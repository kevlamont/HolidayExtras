//------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using HolidayExtras.Models;
using HolidayExtras.Utility;

//------------------------------------------------------------------------------------------------------------------------------

namespace HolidayExtras.Controllers
{
	//--------------------------------------------------------------------------------------------------------------------------

	[Route("holiday_extras/v1/users")]
	[Consumes("application/json")]
	[Produces("application/json")]
	public class UsersController : Controller
	{
		//----------------------------------------------------------------------------------------------------------------------

		private readonly DbaseContext	m_dbase_context;

		//----------------------------------------------------------------------------------------------------------------------

		public UsersController(DbaseContext dbase_context)
		{
			m_dbase_context = dbase_context;
		}

		//----------------------------------------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------------------------------
		//
		// CRUD operations
		//
		//----------------------------------------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Create a new user
		/// </summary>
		/// <param name="new_user"></param> 
		/// <returns>The freshly squeezed User object</returns>
		/// <response code="200">User created</response>
		/// <response code="400">All fields must be provided</response>
		[HttpPost]
		[ProducesResponseType(typeof(User), 200)]
		public IActionResult CreateUser([FromBody] NewUser new_user)
		{
			try
			{
				User    user = Models.User.Create(new_user);

				m_dbase_context.Users.Add(user);
				m_dbase_context.SaveChanges();

				return new ObjectResult(user);
			}
			catch (System.Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Update an extant user
		/// </summary>
		/// <param name="user"></param> 
		/// <returns>The updated User object</returns>
		/// <response code="200">User updated</response>
		/// <response code="404">User id not found</response>
		/// <response code="400">All fields must be provided</response>
		[HttpPut]
		[ProducesResponseType(typeof(User), 200)]
		public IActionResult UpdateUser([FromBody] User user)
		{
			if ((user == null) || (user.m_id == null))
				return BadRequest("The user id cannot be null");

			User    db_user = m_dbase_context.Users.Find(user.m_id); 

			if (db_user == null)
				return NotFound("User id " + user.m_id + " not found");

			try
			{
				db_user.Update(user);

				m_dbase_context.Users.Update(db_user);
				m_dbase_context.SaveChanges();

				return new ObjectResult(db_user);
			}
			catch (System.Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Return an extant user's details
		/// </summary>
		/// <param name="id"></param> 
		/// <returns>The  User object</returns>
		/// <response code="200">Success</response>
		/// <response code="404">User id not found</response>
		[HttpGet]
		[ProducesResponseType(typeof(User), 200)]
		[Route("{id}")]
		public IActionResult SelectUser(Guid id)
		{
			if (id == null)
				return BadRequest("The user id cannot be null");

			User    db_user = m_dbase_context.Users.Find(id);

			if (db_user == null)
				return NotFound("User id " + id + " not found");

			return new ObjectResult(db_user);
		}

		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Delete an extant user
		/// </summary>
		/// <param name="id"></param> 
		/// <returns></returns>
		/// <response code="200">Success</response>
		/// <response code="404">User id not found</response>
		[HttpDelete]
		[Route("{id}")]
		public IActionResult DeleteUser(Guid id)
		{
			if (id == null)
				return BadRequest("The user id cannot be null");

			User    db_user = m_dbase_context.Users.Find(id);

			if (db_user == null)
				return NotFound("User id " + id + " not found");

			m_dbase_context.Remove(db_user);
			m_dbase_context.SaveChanges();

			return Ok();
		}

		//----------------------------------------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------------------------------
		//
		// Searches
		//
		//----------------------------------------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Perform a search for users
		/// </summary>
		/// <remarks>
		/// The given name, family name and email address are scored independently using a case insensitive Jaro-Winkler 
		/// distance, with null or empty parameters ignored.
		/// The results are then sorted by the priority: e-mail match >> family name match >> given name match
		/// </remarks>
		/// <param name="given_name">Optional given name to search</param> 
		/// <param name="family_name">Optional family name to search</param> 
		/// <param name="email_address">Optional e-mail address to search</param> 
		/// <param name="start">The 0 based index of the first result to return</param> 
		/// <param name="count">The maximum number of results to return (1..25 inclusive)</param> 
		/// <returns>The list of found users, in order of best-matching the given search parameters</returns>
		/// <response code="200">Success</response>
		[HttpGet]
		[Route("Searches")]
		public IActionResult SearchUser([FromQuery] int start = 0,
			[FromQuery][RangeAttribute(1, 25)] int count = 25,
			[FromQuery] string given_name = null,
			[FromQuery] string family_name = null,
			[FromQuery] string email_address = null)
		{
			// This is not correct at all: not all users have family names. We need to get the spec
			// for this hammered out.
			//
			// The performance of this is *terrible*. With 1,000,000 users, it takes 19 seconds to
			// return the results.
			//
			// We need to a) optimise the Jaro-Winkler class or b) replace it with something else

			if (start < 0)
				return BadRequest("Start must be zero or greater");

			if ((count < 1) || (count > 25))
				return BadRequest("Count must be between 1 and 25 inclusive");

			PrepSearchParam(ref given_name);
			PrepSearchParam(ref family_name);
			PrepSearchParam(ref email_address);

			JaroWinkler jaro_winkler = new Utility.JaroWinkler();

			var	result = m_dbase_context.Users.OrderBy(x =>
				JaroWinklerEx(jaro_winkler, given_name, x.m_given_name)
				| (JaroWinklerEx(jaro_winkler, family_name, x.m_family_name) << 21)
				| (JaroWinklerEx(jaro_winkler, email_address, x.m_email_address) << 42))
				.Skip(start).Take(count);

			return new ObjectResult(result);
		}

		//----------------------------------------------------------------------------------------------------------------------

		static void PrepSearchParam(ref string value)
		{
			if (value != null)
			{
				value = value.Trim().ToLower();
				if (value.Length == 0)
					value = null;
			}
		}

		//----------------------------------------------------------------------------------------------------------------------

		static ulong JaroWinklerEx(JaroWinkler jaro_winkler, string match, string value)
		{
			if (match == null)
				return 0;

			return (ulong) (((1 << 21) - 1) * jaro_winkler.Distance(match, value.ToLower()));
		}

		//----------------------------------------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------------------------------
		//
		// Test helper code
		//
		//----------------------------------------------------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Populate the user table with a given number of users. 
		/// </summary>
		/// <remarks>
		/// Provided solely to make evaluating the API less onerous.
		/// Only the first 256 generated users will have unique names and e-mail addresses.
		/// </remarks>
		/// <param name="count">The number of users to create</param> 
		/// <response code="200">Success</response>
		[HttpPost]
		[Route("Test")]
		public IActionResult PopulateUsers([FromQuery] int count = 256)
		{
			string[] given_names = new string[]
			{
				"Alberic", "Ninian", "Tottie", "Crispin", "Dorothy", "Ermintrude", "Evadne", "Esau",
				"Dolores", "Lillian", "Euphemia", "Havelock", "Cholmondeley", "Eustace", "Cassandra", "Monmouth",
			};

			string[] family_names = new string[]
			{
				"Haskett", "Cutflower", "De Lish", "Gantt", "Doodad", "Flay", "Cuspcolon", "Crump",
				"Tintwhistle", "Gaunt", "Potato", "Thring", "Groan", "Grabbitas", "Totes", "Flute",
			};

			NewUser new_user = new NewUser();
			int     index = 0;

			for (int i = 0; i < count; ++i)
			{
				int idx_given = index % given_names.Count();
				int idx_family = (index / given_names.Count()) % family_names.Count();

				index += 31; // must be co-prime to the length of the arrays

				new_user.m_given_name = given_names[idx_given];
				new_user.m_family_name = family_names[idx_family];
				new_user.m_email_address = (new_user.m_given_name + "_" + new_user.m_family_name + "@HolidayExtras.com").Replace(' ', '_');

				m_dbase_context.Add(Models.User.Create(new_user));
			}

			m_dbase_context.SaveChanges();

			return Ok();
		}

		//----------------------------------------------------------------------------------------------------------------------
	}

	//--------------------------------------------------------------------------------------------------------------------------
}

//------------------------------------------------------------------------------------------------------------------------------

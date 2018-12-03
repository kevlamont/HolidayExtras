//------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

//------------------------------------------------------------------------------------------------------------------------------

namespace HolidayExtras.Models
{
	//--------------------------------------------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------
	//
	// NewUser
	//
	//--------------------------------------------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// The object type used by POST to /users in order to create a new user
	/// </summary>
	[DataContract]
	public class NewUser
	{
		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// The given name of the user. In the U.K. it is most often the first name, but not always.
		/// Cannot be the empty string.
		/// </summary>
		[DataMember(Order = 2, Name = "given_name")]
		public string m_given_name { get; set; }

		/// <summary>
		/// The family name of the user. In the U.K. it is most often the last name, but not always.
		/// May be the empty string. If the user only has one name, use the given_name field and leave
		/// family_name empty.
		/// </summary>
		[DataMember(Order = 3, Name = "family_name")]
		public string m_family_name { get; set; }

		/// <summary>
		/// The e-mail address of the user.
		/// Cannot be the empty string. Has not been verified as a valid or genuine e-mail address.
		/// </summary>
		[DataMember(Order = 4, Name = "email_address")]
		public string m_email_address { get; set; }

		//----------------------------------------------------------------------------------------------------------------------
	}

	//--------------------------------------------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------
	//
	// User
	//
	//--------------------------------------------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------

	[DataContract]
	public class User
	{
		//----------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// The unique identifier for this user. This is generated automatically by the API when the user is created.
		/// </summary>
		[Key]
		[DataMember(Order = 0, Name = "id")]
		public Guid m_id { get; private set;  }

		/// <summary>
		/// The date and time when the user was created. This is generated automatically by the API when the user is created.
		/// </summary>
		[DataMember(Order = 1, Name = "creation_datetime")]
		public DateTime m_creation_datetime { get; private set; }

		/// <summary>
		/// The given name of the user. In the U.K. it is most often the first name, but not always.
		/// Cannot be the empty string.
		/// </summary>
		[DataMember(Order = 2, Name = "given_name")]
		public string m_given_name { get; set; }

		/// <summary>
		/// The family name of the user. In the U.K. it is most often the last name, but not always.
		/// May be the empty string. If the user only has one name, use the given_name field and leave
		/// family_name empty.
		/// </summary>
		[DataMember(Order = 3, Name = "family_name")]
		public string m_family_name { get; set; }

		/// <summary>
		/// The e-mail address of the user.
		/// Cannot be the empty string, but has not been verified as a valid or genuine e-mail address.
		/// </summary>
		[DataMember(Order = 4, Name = "email_address")]
		public string m_email_address { get; set; }

		//----------------------------------------------------------------------------------------------------------------------
		// Instantiate a new User instance from a NewUser instance. The m_id and m_creation_datatime fields are
		// generated automatically
		//----------------------------------------------------------------------------------------------------------------------
		
		public static User Create(NewUser new_user)
		{
			if ((new_user == null)
				|| (new_user.m_given_name == null) || (new_user.m_family_name == null) || (new_user.m_email_address == null))
			{
				throw new System.Exception("All fields must be provided");
			}

			string given_name = new_user.m_given_name.Trim();
			string family_name = new_user.m_family_name.Trim();
			string email_address = new_user.m_email_address.Trim();

			if ((given_name.Length == 0) || (email_address.Length == 0))
				throw new System.Exception("The user's given name and e-mail address cannot be left blank");

			User    user = new User();

			user.m_id = Guid.NewGuid();
			user.m_creation_datetime = DateTime.Now;
			user.m_given_name = given_name;
			user.m_family_name = family_name;
			user.m_email_address = email_address;

			return user;
		}

		//----------------------------------------------------------------------------------------------------------------------
		// Update the user from the given user
		//----------------------------------------------------------------------------------------------------------------------

		public void Update(User user)
		{
			if ((user == null) || (user.m_id == null) || (user.m_creation_datetime == null)
				|| (user.m_given_name == null) || (user.m_family_name == null) || (user.m_email_address == null))
			{
				throw new System.Exception("All fields must be provided");
			}

			if (user.m_id != m_id)
				throw new System.Exception("Mismatched user ids");

			if (user.m_creation_datetime != m_creation_datetime)
				throw new System.Exception("The creation datetime cannot be changed");

			string given_name = user.m_given_name.Trim();
			string family_name = user.m_family_name.Trim();
			string email_address = user.m_email_address.Trim();

			if ((given_name.Length == 0) || (email_address.Length == 0))
				throw new System.Exception("The user's given name and e-mail address cannot be left blank");

			m_given_name = given_name;
			m_family_name = family_name;
			m_email_address = email_address;
		}

		//----------------------------------------------------------------------------------------------------------------------
	}

	//--------------------------------------------------------------------------------------------------------------------------
}

//------------------------------------------------------------------------------------------------------------------------------

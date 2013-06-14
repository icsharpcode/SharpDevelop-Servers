﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace UsageDataAnalysisWebClient {
	public class WebConfigMembershipProvider : MembershipProvider {

		public override void Initialize(string name,
		  System.Collections.Specialized.NameValueCollection config) {

			base.Initialize(name, config);

		}


		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
			throw new NotImplementedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer) {
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword) {
			throw new NotImplementedException();
		}

		public override string ResetPassword(string username, string answer) {
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user) {
			throw new NotImplementedException();
		}

		public override bool ValidateUser(string username, string password) {

			if (FormsAuthentication.Authenticate(username, password)) {

				//new AuthenticationSuccessEvent(username, this).Raise();

				return true;

			}

			else {

				//new AuthenticationFailureEvent(username, this).Raise();
				return false;

			}

		}

		public override bool UnlockUser(string userName) {
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string username, bool userIsOnline) {
			throw new NotImplementedException();
		}

		public override string GetUserNameByEmail(string email) {
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string username, bool deleteAllRelatedData) {
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline() {
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
			throw new NotImplementedException();
		}

		public override bool EnablePasswordRetrieval {
			get { throw new NotImplementedException(); }
		}

		public override bool EnablePasswordReset {
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresQuestionAndAnswer {
			get { throw new NotImplementedException(); }
		}

		public override string ApplicationName {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override int MaxInvalidPasswordAttempts {
			get { throw new NotImplementedException(); }
		}

		public override int PasswordAttemptWindow {
			get { throw new NotImplementedException(); }
		}

		public override bool RequiresUniqueEmail {
			get { throw new NotImplementedException(); }
		}

		public override MembershipPasswordFormat PasswordFormat {
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredPasswordLength {
			get { throw new NotImplementedException(); }
		}

		public override int MinRequiredNonAlphanumericCharacters {
			get { throw new NotImplementedException(); }
		}

		public override string PasswordStrengthRegularExpression {
			get { throw new NotImplementedException(); }
		}
	}
}
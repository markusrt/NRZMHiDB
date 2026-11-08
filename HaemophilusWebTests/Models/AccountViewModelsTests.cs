using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models
{
    public class AccountViewModelsTests
    {
        // specs/021 US6.3, FR-008: user name, password and role are required; the password has 6 to 100 characters
        // and the confirmation must match
        [TestCase("SV021user", "abcdef", "abcdef", DefaultRoles.User, new string[0])]
        [TestCase(null, "abcdef", "abcdef", DefaultRoles.User, new[] { "UserName" })]
        [TestCase("SV021user", null, null, DefaultRoles.User, new[] { "Password" })]
        [TestCase("SV021user", "abcdef", "abcdef", null, new[] { "RoleName" })]
        [TestCase("SV021user", "abcde", "abcde", DefaultRoles.User, new[] { "Password" })]
        [TestCase("SV021user", "abcdef", "abcdeg", DefaultRoles.User, new[] { "ConfirmPassword" })]
        public void RegisterViewModel_Validation(string userName, string password, string confirmPassword,
            string roleName, string[] invalidMembers)
        {
            var model = new RegisterViewModel
            {
                UserName = userName, Password = password, ConfirmPassword = confirmPassword, RoleName = roleName
            };

            InvalidMembers(model).Should().BeEquivalentTo(invalidMembers);
        }

        // specs/021 FR-008: 100 characters are accepted, 101 are not
        [TestCase(100, true)]
        [TestCase(101, false)]
        public void RegisterViewModel_PasswordMaximumLength(int length, bool valid)
        {
            var password = new string('a', length);
            var model = new RegisterViewModel
            {
                UserName = "SV021user", Password = password, ConfirmPassword = password, RoleName = DefaultRoles.User
            };

            InvalidMembers(model).Any().Should().Be(!valid);
        }

        // specs/021 US8.2, FR-011: the password change requires the current password, a new password of at least
        // 6 characters and a matching confirmation
        [TestCase("old-021", "new021", "new021", new string[0])]
        [TestCase(null, "new021", "new021", new[] { "OldPassword" })]
        [TestCase("old-021", "new02", "new02", new[] { "NewPassword" })]
        [TestCase("old-021", "new021", "new022", new[] { "ConfirmPassword" })]
        public void ManageUserViewModel_Validation(string oldPassword, string newPassword, string confirmPassword,
            string[] invalidMembers)
        {
            var model = new ManageUserViewModel
            {
                OldPassword = oldPassword, NewPassword = newPassword, ConfirmPassword = confirmPassword
            };

            InvalidMembers(model).Should().BeEquivalentTo(invalidMembers);
        }

        // Validated per property, because the comparison rule reports its failure without a member name
        private static IEnumerable<string> InvalidMembers(object model)
        {
            return model.GetType().GetProperties()
                .Where(p => !Validator.TryValidateProperty(p.GetValue(model),
                    new ValidationContext(model) { MemberName = p.Name }, new List<ValidationResult>()))
                .Select(p => p.Name)
                .ToList();
        }
    }
}

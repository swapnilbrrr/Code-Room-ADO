namespace CodeRoom.WebForms.Authentication
{
    public partial class Register
    {
        protected global::System.Web.UI.WebControls.HiddenField CsrfToken;
        protected global::System.Web.UI.WebControls.ValidationSummary ValidationSummary1;
        protected global::System.Web.UI.WebControls.TextBox FullName;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator FullNameRequired;
        protected global::System.Web.UI.WebControls.TextBox Username;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator UsernameRequired;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator UsernameFormat;
        protected global::System.Web.UI.WebControls.TextBox Email;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator EmailRequired;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator EmailFormat;
        protected global::System.Web.UI.WebControls.TextBox Password;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator PasswordRequired;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator PasswordFormat;
        protected global::System.Web.UI.WebControls.TextBox ConfirmPassword;
        protected global::System.Web.UI.WebControls.CompareValidator PasswordMatch;
        protected global::System.Web.UI.WebControls.Label ErrorMessage;
        protected global::System.Web.UI.WebControls.Button RegisterButton;
    }
}

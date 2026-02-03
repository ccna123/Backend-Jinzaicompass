namespace SystemBrightSpotBE.Base
{
    public class BaseConstants
    {
        public const int CodeStatusSuccess = 0;
        public const int CodeStatusFail = 1;
        public const int CodeStatusFailException = 500;
        public const int CodeStatusAuthenticationFailedFail = 401;
        public const int CodeBadRequest = 400;

        public const string StatusSuccess = "success";
        public const string StatusFail = "failed";

        public static class ErrorCode
        {
            public const string UNIQUE = "UNIQUE";
            public const string REFERENCE = "REFERENCE";
            public const string REQUIRED = "REQUIRED";
            public const string NOTFOUND = "NOTFOUND";
            public const string NEWPASSWORDCONFLICT = "NEWPASSWORDCONFLICT";
            public const string VERIFYOLDPASSWORD = "VERIFYOLDPASSWORD";
            public const string OTPUNEXPRIED = "OTPUNEXPRIED";
            public const string OTPEXPRIED = "OTPEXPRIED";
        }
    }
}

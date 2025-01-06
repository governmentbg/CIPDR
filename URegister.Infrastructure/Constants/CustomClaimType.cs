namespace URegister.Infrastructure.Constants
{
    /// <summary>
    /// Видове стойности за ауторизиране на потребител
    /// </summary>
    public static class CustomClaimType
    {
        public static class IdStampit
        {
            public static string PersonalId = "urn:stampit:pid";

            public static string Organization = "urn:stampit:organization";

            public static string PublicKey = "urn:stampit:public_key";

            public static string Certificate = "urn:stampit:certificate";

            public static string CertificateNumber = "urn:stampit:certno";
        }

        public static string FirstName = "urn:io:first_name";
        public static string MiddleName = "urn:io:middle_name";
        public static string LastName = "urn:io:last_name";


        /// <summary>
        /// Имена на потребител
        /// </summary>
        public static string FullName = "urn:io:full_name";

        /// <summary>
        /// Офис
        /// </summary>
        public static string AvOffice = "urn:io:av_office";
    }
}

namespace UserProfileService.Features.Shared
{
    public static class ApiEndpoints
    {
        private const string ApiBase = "api";

        public static class Profiles
        {
            private const string Base = $"{ApiBase}/profiles";

            public const string GetProfile = Base;
            public const string UpdateProfile = Base;
            public const string UploadProfileImage = $"{Base}/image";
        }

        public static class Addresses
        {
            private const string Base = $"{ApiBase}/addresses";

            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{id}}";
            public const string Delete = $"{Base}/{{id}}";
            public const string SetAsDefault = $"{Base}/{{id}}/default";
        }
    }
}

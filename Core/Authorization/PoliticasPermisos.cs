namespace Core.Authorization
{
    public static class PoliticasPermisos
    {
        public const string RequiereCoordinadorAdmin = "RequiereCoordinadorAdmin";
        public const string RequiereAgenteOAdmin = "RequiereAgenteOAdmin";

        public static class RolesSispro
        {
            public const string CoordinadorAdmin = "SECANI-CoordinadorAdmin";
            public const string AgenteSeguimiento = "SECANI-AgenteSeguimiento";
        }
    }
}

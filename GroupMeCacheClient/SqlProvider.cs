namespace GroupMeClientCached
{
    /// <summary>
    /// <see cref="SqlProvider"/> handles static initialization tasks required for SQLite
    /// </summary>
    public static class SqlProvider
    {
        /// <summary>
        /// Initializes static members of the <see cref="SqlProvider"/> class.
        /// </summary>
        static SqlProvider()
        {
            SQLitePCL.Batteries_V2.Init();
        }
    }
}

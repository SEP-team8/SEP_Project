namespace PSPbackend.Helpers
{
    public static class StanGenerator
    {
        public static string GenerateStan()
        {
            return Random.Shared.Next(0, 1_000_000).ToString("D6");
        }

    }
}

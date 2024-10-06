﻿using Soulash2_SaveSync.Integrations;

namespace Soulash2_SaveSync;

internal static class Program
{
    public static void Main(string[] args)
    {
        var im = new IntegrationManager();
        var saveSync = new SaveSync(im.SelectedIntegration);
        saveSync.Start();
    }
}


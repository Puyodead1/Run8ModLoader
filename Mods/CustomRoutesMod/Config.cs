using System.Collections.Generic;

namespace CustomRoutes
{
    public class Config
    {
        public List<CustomRoute> Routes { get; set; } = new List<CustomRoute>
        {
            new CustomRoute { Id = 900, Name = "MyCustomRoute", Region = "CustomRegion" },
            new CustomRoute { Id = 901, Name = "CustomSouthernCARoute", Region = "SouthernCA" }
        };

        public List<string> CustomRegions { get; set; } = new List<string>
        {
            "CustomRegion"
        };
    }

    public class CustomRoute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
    }
}
# CustomRoutes
Adds the ability to load custom routes into the game.

Content\V3Routes folder must contain a folder with the title of the route, and Content\V3Routes\Regions must contain a folder with the title of the region the route belongs to.<br />
Failure to do this will result in the routes not being displayed in the menu

Example:
```json
{
  "Routes": [
    {
      "Id": 900,
      "Name": "MyCustomRoute",
      "Region": "CustomRegion"
    },
    {
      "Id": 901,
      "Name": "CustomSouthernCARoute",
      "Region": "SouthernCA"
    }
  ],
  "CustomRegions": [
    "CustomRegion"
  ]
}
```

In the example above, you would need to create the following folders:
- `Content\V3Routes\Regions\CustomRegion`
- `Content\V3Routes\MyCustomRoute`
- `Content\V3Routes\CustomSouthernCARoute`

You can copy and rename an existing route/region folder and modify its contents to create your custom routes and regions.<br />
This is just an example of loading custom routes, not actually creating them.
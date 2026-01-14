using HarmonyLib;
using Run8ModAPI;
using System;

namespace Dash9LightTweaks
{
    public class C44Dash9WLightsPatch
    {
        private static ILogger _logger;

        public C44Dash9WLightsPatch(Harmony harmony, ILogger logger)
        {
            _logger = logger;

            var targetType = AccessTools.TypeByName("Class795");
            harmony.Patch(
                AccessTools.Method(targetType, "method_118"),
                postfix: new HarmonyMethod(typeof(C44Dash9WLightsPatch), nameof(Postfix))
            );
        }

        static void Postfix(object __instance)
        {
            // Get PowerModel property
            var powerModelProp = AccessTools.Property(__instance.GetType(), "PowerModel");
            var powerModel = (int)powerModelProp.GetValue(__instance);

            // Check if it's C44Dash9W (value 8 in the enum)
            if (powerModel == 8) // LocomotivePowerModel.C44Dash9W
            {
                _logger.Info("C44Dash9W detected, modifying lights...");

                var list2Field = AccessTools.Field(__instance.GetType(), "list_2");
                var list2 = list2Field.GetValue(__instance);

                var listType = list2.GetType();
                var count = (int)listType.GetProperty("Count").GetValue(list2);

                if (count >= 2)
                {
                    var getItem = listType.GetProperty("Item");
                    var lastItem = getItem.GetValue(list2, new object[] { count - 1 });
                    var secondLastItem = getItem.GetValue(list2, new object[] { count - 2 });

                    // Modify both lights
                    ModifyLight(secondLastItem, 1.5f, 5.0f);  // Right light
                    ModifyLight(lastItem, -1.5f, 5.0f);       // Left light

                    _logger.Info("C44Dash9W lights modified successfully");
                }
            }
        }

        static void ModifyLight(object lightItem, float xPos, float newIntensity)
        {
            // Modify intensity (float_4)
            var float4Field = AccessTools.Field(lightItem.GetType(), "float_4");
            float4Field.SetValue(lightItem, newIntensity);

            // Modify position (vector3_1)
            var vec3Field = AccessTools.Field(lightItem.GetType(), "vector3_1");
            var vector3Type = vec3Field.FieldType;

            // Create new Vector3: (X, Y, Z)
            var newVec3 = Activator.CreateInstance(vector3Type,
                xPos,    // X (1.5 or -1.5)
                1.0f,    // Y (moved up from 0.78)
                7.668f   // Z (unchanged)
            );

            vec3Field.SetValue(lightItem, newVec3);
        }
    }
}

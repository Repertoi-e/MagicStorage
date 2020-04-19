using Terraria;
using Terraria.ModLoader;

namespace MagicStoragePlus
{
    public class BlockRecipes : GlobalRecipe
    {
        public static bool active = true;

        // Block recipes if we are in storage access but not if we are in crafting 
        public override bool RecipeAvailable(Recipe recipe)
        {
            if (!active)
            {
                return true;
            }
            try
            {
                var p = StoragePlayer.Get;
                if (p.StorageAccess.X < 0 || p.StorageAccess.Y < 0)
                    return true;
                return p.IsInCrafting();
            }
            catch
            {
                return true;
            }
        }
    }
}
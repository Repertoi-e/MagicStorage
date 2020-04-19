using System.Linq;
using System.Collections.Generic;

using Terraria;

namespace MagicStoragePlus.Sorting
{
    public enum FilterMode
    {
        All,
        Weapons,
        Tools,
        Equipment,
        Potions,
        Placeables,
        Misc
    }

    public enum SortMode
    {
        Default,
        Id,
        Name,
        Quantity
    }

    public static class Items
    {
        static bool FilterName(Item item, string filter)
        {
            return item.Name.ToLowerInvariant().IndexOf(filter.ToLowerInvariant()) >= 0;
        }

        static bool FilterMod(Item item, string filter)
        {
            string mod = "Terraria";
            if (item.modItem != null) mod = item.modItem.mod.DisplayName;
            return mod.ToLowerInvariant().IndexOf(filter.ToLowerInvariant()) >= 0;
        }

        public static IEnumerable<Item> FilterAndSort(IEnumerable<Item> items, SortMode sortMode, FilterMode filterMode, string modFilter, string nameFilter)
        {
            ItemFilter filter;
            switch (filterMode)
            {
                case FilterMode.All: filter = new FilterAll(); break;
                case FilterMode.Weapons: filter = new FilterWeapon(); break;
                case FilterMode.Tools: filter = new FilterTool(); break;
                case FilterMode.Equipment: filter = new FilterEquipment(); break;
                case FilterMode.Potions: filter = new FilterPotion(); break;
                case FilterMode.Placeables: filter = new FilterPlaceable(); break;
                case FilterMode.Misc: filter = new FilterMisc(); break;
                default: filter = new FilterAll(); break;
            }

            IEnumerable<Item> result = items.Where((item) => filter.Passes(item) && FilterMod(item, modFilter) && FilterName(item, nameFilter));
            CompareFunction compare = null;
            switch (sortMode)
            {
                case SortMode.Default: compare = new CompareDefault(); break;
                case SortMode.Id: compare = new CompareID(); break;
                case SortMode.Name: compare = new CompareName(); break;
                case SortMode.Quantity: compare = new CompareID(); break; // Note: We first sort by ID, and down bellow we sort by qunatity 
            };
            if (compare == null) return result;

            BTree<Item> tree = new BTree<Item>(compare);
            foreach (Item item in result)
                tree.Insert(item);

            if (sortMode == SortMode.Quantity)
            {
                BTree<Item> old = tree;
                tree = new BTree<Item>(new CompareQuantity());
                foreach (Item item in old.GetSortedItems())
                    tree.Insert(item);
            }
            return tree.GetSortedItems();
        }

        /*
        public static IEnumerable<Recipe> GetRecipes(SortMode sortMode, FilterMode filterMode, string modFilter, string nameFilter)
        {
            ItemFilter filter;
            switch (filterMode)
            {
                case FilterMode.All: filter = new FilterAll(); break;
                case FilterMode.Weapons: filter = new FilterWeapon(); break;
                case FilterMode.Tools: filter = new FilterTool(); break;
                case FilterMode.Equipment: filter = new FilterEquipment(); break;
                case FilterMode.Potions: filter = new FilterPotion(); break;
                case FilterMode.Placeables: filter = new FilterPlaceable(); break;
                case FilterMode.Misc: filter = new FilterMisc(); break;
                default: filter = new FilterAll(); break;
            }

            var result = Main.recipe.Where((recipe, i) => i < Recipe.numRecipes && filter.Passes(recipe) && FilterMod(recipe.createItem, modFilter) && FilterName(recipe.createItem, nameFilter));

            CompareFunction compare = null;
            switch (sortMode)
            {
                case SortMode.Default: compare = new CompareDefault(); break;
                case SortMode.Id: compare = new CompareID(); break;
                case SortMode.Name: compare = new CompareName(); break;
            };
            if (compare == null) return result;

            BTree<Recipe> tree = new BTree<Recipe>(compare);
            foreach (Recipe recipe in result)
            {
                tree.Insert(recipe);
                if (CraftingGUI.threadNeedsRestart)
                    return new List<Recipe>();
            }
            return tree.GetSortedItems();
        }
        */
    }
}

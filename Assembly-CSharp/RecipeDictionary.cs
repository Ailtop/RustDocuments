using System.Collections.Generic;

public static class RecipeDictionary
{
	private static Dictionary<uint, Dictionary<int, List<Recipe>>> recipeListsDict = new Dictionary<uint, Dictionary<int, List<Recipe>>>();

	public static void CacheRecipes(RecipeList recipeList)
	{
		if (recipeList == null || recipeListsDict.TryGetValue(recipeList.FilenameStringId, out var _))
		{
			return;
		}
		Dictionary<int, List<Recipe>> dictionary = new Dictionary<int, List<Recipe>>();
		recipeListsDict.Add(recipeList.FilenameStringId, dictionary);
		Recipe[] recipes = recipeList.Recipes;
		foreach (Recipe recipe in recipes)
		{
			List<Recipe> value2 = null;
			if (!dictionary.TryGetValue(recipe.Ingredients[0].Ingredient.itemid, out value2))
			{
				value2 = new List<Recipe>();
				dictionary.Add(recipe.Ingredients[0].Ingredient.itemid, value2);
			}
			value2.Add(recipe);
		}
	}

	public static Recipe GetMatchingRecipeAndQuantity(RecipeList recipeList, List<Item> orderedIngredients, out int quantity)
	{
		quantity = 0;
		if (recipeList == null)
		{
			return null;
		}
		if (orderedIngredients == null || orderedIngredients.Count == 0)
		{
			return null;
		}
		List<Recipe> recipesByFirstIngredient = GetRecipesByFirstIngredient(recipeList, orderedIngredients[0]);
		if (recipesByFirstIngredient == null)
		{
			return null;
		}
		foreach (Recipe item2 in recipesByFirstIngredient)
		{
			if (item2 == null || item2.Ingredients.Length != orderedIngredients.Count)
			{
				continue;
			}
			bool flag = true;
			int num = int.MaxValue;
			int num2 = 0;
			Recipe.RecipeIngredient[] ingredients = item2.Ingredients;
			for (int i = 0; i < ingredients.Length; i++)
			{
				Recipe.RecipeIngredient recipeIngredient = ingredients[i];
				Item item = orderedIngredients[num2];
				if (recipeIngredient.Ingredient != item.info || item.amount < recipeIngredient.Count)
				{
					flag = false;
					break;
				}
				int num3 = item.amount / recipeIngredient.Count;
				if (num2 == 0)
				{
					num = num3;
				}
				else if (num3 < num)
				{
					num = num3;
				}
				num2++;
			}
			if (flag)
			{
				quantity = num;
				if (quantity > 1 && !item2.CanQueueMultiple)
				{
					quantity = 1;
				}
				return item2;
			}
		}
		return null;
	}

	private static List<Recipe> GetRecipesByFirstIngredient(RecipeList recipeList, Item firstIngredient)
	{
		if (recipeList == null)
		{
			return null;
		}
		if (firstIngredient == null)
		{
			return null;
		}
		if (!recipeListsDict.TryGetValue(recipeList.FilenameStringId, out var value))
		{
			CacheRecipes(recipeList);
		}
		if (value == null)
		{
			return null;
		}
		if (!value.TryGetValue(firstIngredient.info.itemid, out var value2))
		{
			return null;
		}
		return value2;
	}

	public static bool ValidIngredientForARecipe(Item ingredient, RecipeList recipeList)
	{
		if (ingredient == null)
		{
			return false;
		}
		if (recipeList == null)
		{
			return false;
		}
		Recipe[] recipes = recipeList.Recipes;
		foreach (Recipe recipe in recipes)
		{
			if (!(recipe == null) && recipe.ContainsItem(ingredient))
			{
				return true;
			}
		}
		return false;
	}
}

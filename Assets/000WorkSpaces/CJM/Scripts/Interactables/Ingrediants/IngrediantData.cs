using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class IngrediantData : IUsableId
{
    public string ID;
    public string Name_KR;
    public string Name_EN;
    public float Price;
    public Sprite Sprite;

    public string GetId()
    {
        return ID;
    }
}
public partial class DataManager
{
    public DataTableParser<IngrediantData> Ingrediant;

    // Addressable 에셋 주소
    private const string _ingrediantDataAdress = "IngrediantDataSheetCsv";

    public async void IngrediantDataInitRoutine()
    {
        string dataCsv;
        dataCsv = await GetDataString(true, _ingrediantDataAdress);

        Ingrediant = new DataTableParser<IngrediantData>((words, dict) =>
        {
            IngrediantData ingrediant = new();

            ingrediant.ID = words[dict["ID"]];
            ingrediant.Name_KR = words[dict["Name_KR"]];
            ingrediant.Name_EN = words[dict["Name_EN"]];
            float.TryParse(words[dict["Cost"]], out ingrediant.Price);

            if (Addressables.ResourceLocators.Any(locator => locator.Locate($"Sprite/{ingrediant.ID}.png", typeof(Sprite), out var locations)))
            {
                Addressables.LoadAssetAsync<Sprite>($"Sprite/{ingrediant.ID}.png").Completed += task =>
                {
                    ingrediant.Sprite = task.Result;
                };
            }
            else
            {
                Debug.LogError($"[{ingrediant.ID}] Sprite경로:[Sprite/{ingrediant.ID}.png] Addressable주소에 해당 재료의 Sprite가 없습니다");
            }
            return ingrediant;
        });

        Ingrediant.Load(dataCsv);
    }
}
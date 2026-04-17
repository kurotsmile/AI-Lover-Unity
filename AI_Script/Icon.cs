using UnityEngine;

public class Icon : MonoBehaviour
{
    [Header("Main Obj")]
    public App app;

    public void On_load()
    {
        PlayerPrefs.DeleteKey("s_data_icon_temp");
        PlayerPrefs.DeleteKey("s_data_json_icon_offline");
        PlayerPrefs.DeleteKey("s_data_json_icon_category_offline");
    }
}

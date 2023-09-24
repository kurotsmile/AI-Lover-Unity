using UnityEngine;

public class character_obj : MonoBehaviour
{
    public Sprite icon;
    public bool is_free;
    public bool is_materials_1=true;
    public bool is_materials_2=false;
    public SkinnedMeshRenderer skinned_costumes;
    public SkinnedMeshRenderer skinned_costumes2;
    public SkinnedMeshRenderer skinned_head;
    public string s_type_costumes;
    public string s_type_head;

    public void OnCallMusicPlay()
    {

    }

    public void set_skinned_costumes(Texture2D data_img)
    {
        if(is_materials_1) this.skinned_costumes.materials[0].mainTexture = data_img;
        if (is_materials_2)
        {
            if(this.skinned_costumes.materials.Length>1) this.skinned_costumes.materials[1].mainTexture = data_img;
        }
        if (skinned_costumes2!=null) this.skinned_costumes2.materials[0].mainTexture = data_img;
    }

    public void set_skinned_head(Texture2D data_img)
    {
        this.skinned_head.materials[0].mainTexture = data_img;
    }
}

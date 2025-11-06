// 파일: Assets/Editor/CreateSpriteMaterial.cs
using UnityEngine;
using UnityEditor;

public class CreateSpriteMaterial
{
    [MenuItem("Tools/Create Sprite Material")]
    static void CreateMat()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            EditorUtility.DisplayDialog("오류", "사용 가능한 Sprite 셰이더를 찾을 수 없습니다.", "확인");
            return;
        }

        Material mat = new Material(shader);
        string path = "Assets/Sprite_Material.mat";
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = mat;
        EditorUtility.DisplayDialog("완료", $"Material 생성: {path}", "확인");
    }
}

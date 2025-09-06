using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // required for editor functionality
#endif

/*
Create two sets of objects: 5 cubes and 5 spheres.
Create a custom editor for them that has these actions as buttons:
Select all cubes/spheres
Clear selection
Disable/Enable all cubes/spheres (colorized based on whether the objects are enabled or disabled)
Also, designate a variable for the size of the cube/sphere. 
Create warnings on the inspector component for when this variable is edited (e.g., "The cubes' sizes cannot be bigger than 2!" "The spheres' radius cannot be smaller than 1!" etc.). You can come up with warning content.
*/

public class EnemyBehavior : MonoBehaviour
{
    // Enum to differentiate enemy types
    public enum EnemyType { CubeEnemy, SphereEnemy }

    [Header("Enemy Setup")]
    public EnemyType type = EnemyType.CubeEnemy;//type of enemy for this manager
    public int count = 5; //number of enemies to spawn
    public float size = 1f;//cube size or sphere radius

    [Header("Enemy Status")]
    [SerializeField] private float health; //enemy health
    [SerializeField] private int attackPt; //attack points

    //inner class to identify enemy instances

    public class EnemyMarker : MonoBehaviour
    {
        public EnemyType type;//type of enemy this marker represents
  
    }

    //Function with ability to build or refresh enemies
    public void BuildOrRefresh()
    {
        // Find all children with EnemyMarker of this type
        EnemyMarker[] markers = GetComponentsInChildren<EnemyMarker>(true);
        for (int i = markers.Length - 1; i >= 0; i--)
        {
            if (markers[i].type == type)
            {
                if (Application.isPlaying)//for play mode
                    Destroy(markers[i].gameObject);
                else
                    DestroyImmediate(markers[i].gameObject);//for editor mode
            }
        }

        // Create new enemies
        for (int i = 0; i < count; i++)
        {
            //primitive type based on enum
            GameObject go = GameObject.CreatePrimitive(
                type == EnemyType.CubeEnemy ? PrimitiveType.Cube : PrimitiveType.Sphere
            );

            go.name = $"{type}_{i + 1}";//name object built based on type and order
            go.transform.SetParent(transform, false);//make object child of the manager (Cube or Sphere Managers)
            go.transform.localPosition = new Vector3(i * 1.5f, 0f, 0f);//position in scene along x axis spread apart
            go.transform.localScale = type == EnemyType.CubeEnemy ? Vector3.one * size : Vector3.one * size * 2f;//scale for clarity

            //Identify enemy type with marker
            EnemyMarker marker = go.AddComponent<EnemyMarker>();
            marker.type = type;
        }
    }
}

#if UNITY_EDITOR
//Cusomize editor
[CustomEditor(typeof(EnemyBehavior)), CanEditMultipleObjects]
public class EnemyBehaviorEditor : Editor
{
    //Serialize properties to allow changes in inspector
    SerializedProperty typeProp;
    SerializedProperty countProp;
    SerializedProperty sizeProp;
    SerializedProperty healthProp;
    SerializedProperty attackPtProp;

    //Initialize the properties
    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("type");
        countProp = serializedObject.FindProperty("count");
        sizeProp = serializedObject.FindProperty("size");
        healthProp = serializedObject.FindProperty("health");
        attackPtProp = serializedObject.FindProperty("attackPt");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //draw these basic properties
        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(countProp);

        //show cube or sphere size depending on which Manager is selected in hierarchy
        EditorGUILayout.PropertyField(
            sizeProp,
            new GUIContent((EnemyBehavior.EnemyType)typeProp.enumValueIndex == EnemyBehavior.EnemyType.CubeEnemy ? "Cube Size" : "Sphere Radius")
        );

        EditorGUILayout.Space(5);

        //Draw health prop, warning if value is less than 0
        EditorGUILayout.PropertyField(healthProp);
        if (healthProp.floatValue < 0f)
        {
            EditorGUILayout.HelpBox("The health cannot be less than 0!", MessageType.Warning);
        }

        //Draw other properties
        EditorGUILayout.PropertyField(attackPtProp, new GUIContent("Attack Pt"));

        EditorGUILayout.Space(10);

        //Warnings for sizes less than 1 or greater than 2
        if (sizeProp.floatValue > 2f) 
            EditorGUILayout.HelpBox("The enemies' size cannot be bigger than 2!", MessageType.Warning);

        if (sizeProp.floatValue < 1f)
            EditorGUILayout.HelpBox("The enemies' size cannot be smaller than 1!", MessageType.Warning);

        //Button to build or refresh enemies/cubes or spheres
        if (GUILayout.Button("Build/Refresh", GUILayout.Height(30)))
        {
            foreach (Object t in targets)
            {
                EnemyBehavior e = (EnemyBehavior)t;
                Undo.RegisterFullObjectHierarchyUndo(e.gameObject, "Build/Refresh Enemies");
                e.BuildOrRefresh();//call function to spawn enemies
                EditorUtility.SetDirty(e);//used in Unity editor scripts to mark or flag as changed
            }
        }

        EditorGUILayout.Space(10);

        //Buttons for selecting or clearing
        using (new EditorGUILayout.HorizontalScope())
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);//gray background
            if (GUILayout.Button("Select all enemies"))
            {
                SelectAllEnemies((EnemyBehavior.EnemyType)typeProp.enumValueIndex);
            }

            if (GUILayout.Button("Clear selection"))
            {
                Selection.objects = new Object[0];//clear the selection
            }
            GUI.backgroundColor = originalColor;
        }

        // Enable/Disable all enemies with green or red background
        bool allActive = AllActive((EnemyBehavior.EnemyType)typeProp.enumValueIndex);
        Color cached = GUI.backgroundColor;
        GUI.backgroundColor = allActive ? Color.red : Color.green; // Color reflects action: red = disabling and green = enabling

        string buttonText = allActive ? "Disable all" : "Enable all";//text for button
        if (GUILayout.Button(buttonText, GUILayout.Height(25)))
            ToggleAll((EnemyBehavior.EnemyType)typeProp.enumValueIndex, !allActive);

        GUI.backgroundColor = cached;

        serializedObject.ApplyModifiedProperties();//apply changes
    }

    // Select all enemies of a specific type in the scene
    private void SelectAllEnemies(EnemyBehavior.EnemyType type)
    {
        EnemyBehavior.EnemyMarker[] allMarkers = FindObjectsOfType<EnemyBehavior.EnemyMarker>(true);
        var selected = new System.Collections.Generic.List<Object>();
        foreach (EnemyBehavior.EnemyMarker m in allMarkers)
        {
            if (m.type == type)
            {
                selected.Add(m.gameObject);
            }
        }
        Selection.objects = selected.ToArray();
    }

    // Check if all enemies of a type are active
    private bool AllActive(EnemyBehavior.EnemyType type)
    {
        EnemyBehavior.EnemyMarker[] allMarkers = FindObjectsOfType<EnemyBehavior.EnemyMarker>(true);
        bool foundAny = false;
        foreach (EnemyBehavior.EnemyMarker m in allMarkers)
        {
            if (m.type == type)
            {
                foundAny = true;
                if (!m.gameObject.activeSelf) return false;
            }
        }
        return foundAny;
    }

    // Enable or disable all enemies of a type
    private void ToggleAll(EnemyBehavior.EnemyType type, bool enable)
    {
        EnemyBehavior.EnemyMarker[] allMarkers = FindObjectsOfType<EnemyBehavior.EnemyMarker>(true);
        foreach (EnemyBehavior.EnemyMarker m in allMarkers)
        {
            if (m.type == type)
            {
                Undo.RecordObject(m.gameObject, enable ? "Enable Enemy" : "Disable Enemy");
                m.gameObject.SetActive(enable);//toggle
            }
        }
    }
}
#endif
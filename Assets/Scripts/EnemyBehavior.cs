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
    // enum to differentiate enemy or shape types
    public enum EnemyType { CubeEnemy, SphereEnemy }

    [Header("Enemy Setup")]
    public EnemyType type = EnemyType.CubeEnemy;
    public int count = 5; //5 cube enemies
    public float size = 1f;

public class EnemyMarker : MonoBehaviour
{ 
    
}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

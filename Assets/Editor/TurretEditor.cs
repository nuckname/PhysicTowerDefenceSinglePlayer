using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Turret))]
public class TurretEditor : Editor
{
    // Temporary reference to hold the card in the inspector before we press 'Add'
    private GridData _cardToAdd;

    public override void OnInspectorGUI()
    {
        // 1. Draw the default Turret variables so you don't lose the standard Inspector
        DrawDefaultInspector();

        // Get a reference to the specific Turret we are inspecting
        Turret turret = (Turret)target;

        // 2. Add some visual spacing and a header
        GUILayout.Space(15);
        EditorGUILayout.LabelField("Testing & Debugging", EditorStyles.boldLabel);

        // 3. Create a horizontal row for our custom tools
        EditorGUILayout.BeginHorizontal();
        
        // Create an object picker field specifically for TurretCards
        _cardToAdd = (GridData)EditorGUILayout.ObjectField("Select Card", _cardToAdd, typeof(GridData), false);

        // Create the button that triggers the add function
        if (GUILayout.Button("Add to Inventory", GUILayout.Width(120)))
        {
            if (_cardToAdd != null)
            {
                // Record this action so you can use Ctrl+Z (Undo) in the editor
                Undo.RecordObject(turret, "Add Turret Card");
                
                // Call the existing method on your Turret script
                turret.AddCardToInventory(_cardToAdd);
                
                // Mark the turret as 'dirty' so Unity knows to save this new list state
                EditorUtility.SetDirty(turret);
                
                // Clear the slot so you can easily add the next one
                _cardToAdd = null; 
            }
            else
            {
                Debug.LogWarning("Please select a TurretCard in the slot before adding.");
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Rotator : MonoBehaviour {
    [SerializeField] private Vector3 rotationAmount;
    [SerializeField] private float rotationSpeed = 1.0f;
    
    [OnValueChanged("@transform.localRotation = UnityEngine.Quaternion.identity")]
    [SerializeField] private bool rotate;

    
    private void Update() {
        if(!rotate) return;
        transform.localRotation *= Quaternion.Euler(rotationSpeed * Time.deltaTime * rotationAmount);
    }

    private void OnDrawGizmos() {
        #if UNITY_EDITOR
        if(Application.isPlaying || !rotate) return;
        EditorApplication.QueuePlayerLoopUpdate();
        SceneView.RepaintAll();
        #endif
    }
}
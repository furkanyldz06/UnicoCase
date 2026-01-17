using UnityEngine;

namespace BoardDefence.Utils
{
    /// <summary>
    /// Utility component to automatically destroy a GameObject after a delay
    /// Useful for projectiles and visual effects
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 2f;

        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}


using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Utils
{

    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifetime = 2f;
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private GameObject _hitEffectPrefab;

        private Vector2 _direction;
        private Rigidbody2D _rigidbody;
        private float _timer;
        private bool _isActive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
        }


        public void Fire(Vector2 direction, int damage)
        {
            _direction = direction.normalized;
            _damage = damage;
            _timer = _lifetime;
            _isActive = true;
            
            _rigidbody.linearVelocity = _direction * _speed;
            
            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }

        private void Update()
        {
            if (!_isActive) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            // Check if hit a target
            if (((1 << other.gameObject.layer) & _targetLayer) != 0)
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    damageable.TakeDamage(_damage);
                    OnHit();
                }
            }
        }

        private void OnHit()
        {
            _isActive = false;
            _rigidbody.linearVelocity = Vector2.zero;

            if (_hitEffectPrefab != null)
            {
                Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}


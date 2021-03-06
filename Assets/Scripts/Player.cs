using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public float _speed = 3.5f;
    private float _speedMultiplier = 1f;
    [SerializeField]
    public GameObject _laserPrefab;
    [SerializeField]
    public GameObject _tripleShotPrefab;
    [SerializeField]
    private float _fireRate = 0.15f;
    private float _canFire = -1f;
    private int _ammoCount = 15;
    private int _lives = 3;
    private int _shieldHealth = 3;
    private SpawnManager _spawnManager;
    private bool _isTripleShotActive = false;
    private bool _isSpeedActive = false;
    private bool _isShieldActive = false;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private GameObject _rightEngine, _leftEngine;
    private int score;
    private UIManager _uiManager;
    [SerializeField]
    private AudioClip _laserSoundClip;
    private AudioSource _audioSource;

    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        _audioSource = GetComponent<AudioSource>();

        if (_spawnManager == null)
        {
            Debug.LogError("The Spawn Manager is NULL.");
        }

        if (_uiManager == null)
        {
            Debug.LogError("The UI Manager is NULL.");
        }

        if (_audioSource == null)
        {
            Debug.LogError("The Audio Source on the player is NULL.");
        }
        else
        {
            _audioSource.clip = _laserSoundClip;
        }
    }

    void Update()
    {
        CalculateMovement();
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            FireLaster();
        }
    }

    void CalculateMovement()
    {
        float horizantalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizantalInput, verticalInput, 0);
        transform.Translate(direction * _speed * _speedMultiplier * Time.deltaTime);
        _speedMultiplier = 1f;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speedMultiplier = 1.5f;
        }

        if (transform.position.y >= 0)
        {
            transform.position = new Vector3(transform.position.x, 0, 0);
        }
        else if (transform.position.y <= -3.8f)
        {
            transform.position = new Vector3(transform.position.x, -3.8f, 0);
        }

        if (transform.position.x >= 11.3f)
        {
            transform.position = new Vector3(-11.3f, transform.position.y, 0);
        }
        else if (transform.position.x <= -11.3f)
        {
            transform.position = new Vector3(11.3f, transform.position.y, 0);
        }
    }

    void FireLaster()
    {
        _canFire = Time.time + _fireRate;

        if (_ammoCount > 0)
        {
            if (_isTripleShotActive == true)
            {
                Instantiate(_tripleShotPrefab, transform.position + new Vector3(0, 1.05f, 0), Quaternion.identity);
            }
            else
            {
                Instantiate(_laserPrefab, transform.position + new Vector3(0, 1.05f, 0), Quaternion.identity);
            }

            _audioSource.Play();
            _ammoCount = _ammoCount - 1;
            _uiManager.UpdateAmmo(_ammoCount);
        }
    }

    public void Damage()
    {
        if (_isShieldActive == true)
        {
            _shieldHealth = _shieldHealth - 1;

            if (_shieldHealth == 2)
            {
                _shieldVisualizer.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .67f);
            }
            if (_shieldHealth == 1)
            {
                _shieldVisualizer.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .33f);
            }
            if (_shieldHealth == 0)
            {
                _isShieldActive = false;
                _shieldVisualizer.SetActive(false);
            }
            return;
        }

        _lives--;

        if (_lives == 2)
        {
            _leftEngine.SetActive(true);
        }
        else if (_lives == 1)
        {
            _rightEngine.SetActive(true);
        }

        _uiManager.UpdateLives(_lives);
        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();
            Destroy(this.gameObject);
        }
    }

    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5f);
        _isTripleShotActive = false;
    }

    public void SpeedBoostActive()
    {
        _isSpeedActive = true;
        _speed *= 2;
        StartCoroutine(SpeedBoostPowerDownRoutine());
    }

    IEnumerator SpeedBoostPowerDownRoutine()
    {
        yield return new WaitForSeconds(5f);
        _isSpeedActive = false;
        _speed = 3.5f;
    }

    public void ShieldActive()
    {
        _isShieldActive = true;
        _shieldHealth = 3;
        _shieldVisualizer.SetActive(true);
        _shieldVisualizer.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }

    public void AddToScore(int points)
    {
        score += points;
        _uiManager.UpdateScore(score);
    }

    public void AmmoRefill()
    {
        _ammoCount = 15;
        _uiManager.UpdateAmmo(_ammoCount);
    }

    public void Heal()
    {
        if (_lives < 3)
        {
            _lives = _lives + 1;
            _uiManager.UpdateLives(_lives);
        }

        if (_lives == 2)
        {
            _leftEngine.SetActive(true);
            _rightEngine.SetActive(false);
        }
        else if (_lives == 1)
        {
            _rightEngine.SetActive(true);
            _leftEngine.SetActive(true);
        }
    }
}
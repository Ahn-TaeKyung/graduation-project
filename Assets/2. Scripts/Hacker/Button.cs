using System.Collections;
using UnityEngine;
namespace hacker
{
    public class Button : MonoBehaviour
    {
        [Header("��ư ���� �Ÿ�")]
        public float pressDistance = 1;
        [Header("���� ���� (��: -z)")]
        public Vector3 pressDirection = Vector3.back;
        private float holdThreshold = 0.3f;

        private Vector3 defaultPosition;
        private bool isPressed = false;
        private float pressTime;

        private ModuleZoom ModuleZoom;
        private ClickDragHandler clickDragHandler;
        private Camera hackerCamera;
        private ButtonPatternManager patternManager;
        private Collider myCollider;   // Inspector���� �Ҵ�(Ȥ�� GetComponent�� �Ҵ�)
        private new Renderer renderer;

        void Awake()
        {
            SetAllColors(Color.grey);
            defaultPosition = transform.localPosition;
            myCollider = GetComponent<Collider>();
            GameObject hackerCameraObject = GameObject.FindGameObjectWithTag("hacker");
            if (hackerCameraObject != null)
            {
                hackerCamera = hackerCameraObject.GetComponent<Camera>();
            }
            else
            {
                Debug.LogError("�±װ� 'hacker'�� ī�޶� ã�� �� �����ϴ�.");
            }
            ModuleZoom = GetComponentInParent<ModuleZoom>();
            if (patternManager == null)
                patternManager = GetComponent<ButtonPatternManager>();
            if (patternManager != null)
            {
                patternManager.patternCode = GenerateRandomCode();
                patternManager.ApplyPatternCode(); // �ڵ� �ٲ������ ���� ���� ����
                Debug.Log($"[Button] �� �ڵ�: {patternManager.patternCode}");
            }
            clickDragHandler = GetComponent<ClickDragHandler>();
            if (clickDragHandler == null)
                clickDragHandler = gameObject.AddComponent<ClickDragHandler>();

            clickDragHandler.OnLeftPress += OnButtonPress;
            clickDragHandler.OnLeftRelease += OnButtonRelease;  // �� ��(�ö�)

        }
        private void Update()
        {
            myCollider.enabled = ModuleZoom.c_zoomed;
            if (patternManager.isComplete == true)
            {
                myCollider.enabled = false;
                SetAllColors(Color.green);
            }
        }

        void OnDestroy()
        {
            // �޸𸮸� ����
            if (clickDragHandler != null)
            {
                clickDragHandler.OnLeftPress -= OnButtonPress;
                clickDragHandler.OnLeftRelease -= OnButtonRelease;  // �� ��(�ö�)

            }
        }


        void OnButtonPress()
        {
            if (!ModuleZoom.c_zoomed) return;
            // Raycast�� �ڱ� �ڽ� ������ Ȯ��
            Ray ray = hackerCamera.ScreenPointToRay(clickDragHandler.LastClickPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.transform == this.transform)
                {
                    // ��ư�� �߽��� ī�޶� View �ȿ� �ִ��� Ȯ��
                    Vector3 viewportPos = hackerCamera.WorldToViewportPoint(transform.position);

                    bool isVisible =
                        viewportPos.z > 0 && // ī�޶� �տ� �־�� ��
                        viewportPos.x > 0 && viewportPos.x < 1 &&
                        viewportPos.y > 0 && viewportPos.y < 1;

                    if (isVisible)
                    {
                        patternManager.OnInputStarted();
                        pressTime = Time.time;
                        SetAllColors(Color.white);
                        ButtonPress();
                    }
                    // ������ ������ ����
                }
            }
        }


        void OnButtonRelease()
        {
            if (!ModuleZoom.c_zoomed) return;
            if (!isPressed) return;

            float holdTime = Time.time - pressTime;

            if (holdTime <= holdThreshold)
            {
                // Ŭ�� ó��
                patternManager.OnButtonClick();
                Debug.Log($"[Button] Click ����, �ð�:{holdTime:F2}s");
            }
            else
            {
                // Ȧ�� ó��
                patternManager.OnButtonHold(holdTime);
                Debug.Log($"[Button] Hold ����, �ð�:{holdTime:F2}s");
            }

            ButtonRelease();
        }

        void ButtonPress()
        {
            if (!isPressed)
            {
                transform.localPosition = defaultPosition + pressDirection.normalized * pressDistance;
                isPressed = true;
                Debug.Log($"[Button:{gameObject.name}] ����");
            }
        }

        void ButtonRelease()
        {
            if (isPressed)
            {
                transform.localPosition = defaultPosition;
                isPressed = false;
                Debug.Log($"[Button:{gameObject.name}] ����");
            }
        }

        public static string GenerateRandomCode(int length = 6)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            System.Random rand = new System.Random(System.DateTime.Now.Millisecond + UnityEngine.Random.Range(0, 100000));
            char[] code = new char[length];
            for (int i = 0; i < length; i++)
                code[i] = chars[rand.Next(62)];
            return new string(code);
        }
        public void SetAllColors(Color color)
        {
            // �ڽŰ� ��� �ڽ��� Renderer(=MeshRenderer ��) ������Ʈ�� ��� ã��
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.color = color;
            }
        }
    }
}
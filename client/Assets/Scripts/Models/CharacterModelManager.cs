using UnityEngine;

namespace LegendsOfTianming.Core
{
    public class CharacterModelManager : MonoBehaviour
    {
        [Header("Character Models")]
        public GameObject azureCloudModel;
        public GameObject ironBellModel;
        public GameObject shadowVeilModel;
        public GameObject wanderingSpearModel;
        public GameObject spiritLuteModel;
        public GameObject stoneheartModel;
        
        [Header("Weapons")]
        public GameObject swordWeapon;
        public GameObject staffWeapon;
        public GameObject daggerWeapon;
        public GameObject spearWeapon;
        public GameObject luteWeapon;
        public GameObject hammerWeapon;
        
        private Character character;
        private GameObject currentModel;
        private GameObject currentWeapon;
        private Animator animator;

        private void Start()
        {
            character = GetComponent<Character>();
            if (character != null)
            {
                SetupCharacterModel(character.characterClass);
            }
        }

        public void SetupCharacterModel(string characterClass)
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
            }
            
            GameObject modelPrefab = GetModelPrefab(characterClass);
            if (modelPrefab != null)
            {
                currentModel = Instantiate(modelPrefab, transform);
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.identity;
                
                animator = currentModel.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = currentModel.AddComponent<Animator>();
                    SetupAnimatorController(characterClass);
                }
                
                SetupWeapon(characterClass);
                SetupCharacterComponents();
            }
            else
            {
                CreatePlaceholderModel(characterClass);
            }
        }

        private GameObject GetModelPrefab(string characterClass)
        {
            switch (characterClass)
            {
                case "Azure Cloud Sect": return azureCloudModel;
                case "Iron Bell Sect": return ironBellModel;
                case "Shadow Veil Sect": return shadowVeilModel;
                case "Wandering Spear Sect": return wanderingSpearModel;
                case "Spirit Lute Sect": return spiritLuteModel;
                case "Stoneheart Sect": return stoneheartModel;
                default: return null;
            }
        }

        private void CreatePlaceholderModel(string characterClass)
        {
            currentModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            currentModel.transform.SetParent(transform);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            
            Renderer renderer = currentModel.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = GetClassColor(characterClass);
                renderer.material = material;
            }
            
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(currentModel.transform);
            head.transform.localPosition = new Vector3(0, 0.8f, 0);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material = renderer.material;
            }
            
            SetupWeapon(characterClass);
            SetupCharacterComponents();
            
            Debug.Log($"🎭 Created placeholder model for {characterClass}");
        }

        private Color GetClassColor(string characterClass)
        {
            switch (characterClass)
            {
                case "Azure Cloud Sect": return Color.cyan;
                case "Iron Bell Sect": return Color.gray;
                case "Shadow Veil Sect": return Color.black;
                case "Wandering Spear Sect": return Color.red;
                case "Spirit Lute Sect": return Color.magenta;
                case "Stoneheart Sect": return Color.yellow;
                default: return Color.white;
            }
        }

        private void SetupWeapon(string characterClass)
        {
            if (currentWeapon != null)
            {
                Destroy(currentWeapon);
            }
            
            GameObject weaponPrefab = GetWeaponPrefab(characterClass);
            if (weaponPrefab != null)
            {
                Transform weaponAttachPoint = FindWeaponAttachPoint();
                if (weaponAttachPoint != null)
                {
                    currentWeapon = Instantiate(weaponPrefab, weaponAttachPoint);
                    currentWeapon.transform.localPosition = Vector3.zero;
                    currentWeapon.transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                CreatePlaceholderWeapon(characterClass);
            }
        }

        private GameObject GetWeaponPrefab(string characterClass)
        {
            switch (characterClass)
            {
                case "Azure Cloud Sect": return swordWeapon;
                case "Iron Bell Sect": return staffWeapon;
                case "Shadow Veil Sect": return daggerWeapon;
                case "Wandering Spear Sect": return spearWeapon;
                case "Spirit Lute Sect": return luteWeapon;
                case "Stoneheart Sect": return hammerWeapon;
                default: return null;
            }
        }

        private void CreatePlaceholderWeapon(string characterClass)
        {
            Transform weaponAttachPoint = FindWeaponAttachPoint();
            if (weaponAttachPoint == null) return;
            
            switch (characterClass)
            {
                case "Azure Cloud Sect":
                    currentWeapon = CreateSwordPlaceholder(weaponAttachPoint);
                    break;
                case "Iron Bell Sect":
                    currentWeapon = CreateStaffPlaceholder(weaponAttachPoint);
                    break;
                case "Shadow Veil Sect":
                    currentWeapon = CreateDaggerPlaceholder(weaponAttachPoint);
                    break;
                case "Wandering Spear Sect":
                    currentWeapon = CreateSpearPlaceholder(weaponAttachPoint);
                    break;
                case "Spirit Lute Sect":
                    currentWeapon = CreateLutePlaceholder(weaponAttachPoint);
                    break;
                case "Stoneheart Sect":
                    currentWeapon = CreateHammerPlaceholder(weaponAttachPoint);
                    break;
            }
        }

        private GameObject CreateSwordPlaceholder(Transform parent)
        {
            GameObject sword = new GameObject("Sword");
            sword.transform.SetParent(parent);
            sword.transform.localPosition = Vector3.zero;
            sword.transform.localRotation = Quaternion.identity;
            
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.SetParent(sword.transform);
            blade.transform.localPosition = new Vector3(0, 0.5f, 0);
            blade.transform.localScale = new Vector3(0.1f, 1f, 0.05f);
            
            Renderer bladeRenderer = blade.GetComponent<Renderer>();
            if (bladeRenderer != null)
            {
                Material bladeMaterial = new Material(Shader.Find("Standard"));
                bladeMaterial.color = Color.white;
                bladeMaterial.SetFloat("_Metallic", 0.8f);
                bladeRenderer.material = bladeMaterial;
            }
            
            GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hilt.transform.SetParent(sword.transform);
            hilt.transform.localPosition = Vector3.zero;
            hilt.transform.localScale = new Vector3(0.15f, 0.2f, 0.15f);
            
            return sword;
        }

        private GameObject CreateStaffPlaceholder(Transform parent)
        {
            GameObject staff = new GameObject("Staff");
            staff.transform.SetParent(parent);
            staff.transform.localPosition = Vector3.zero;
            staff.transform.localRotation = Quaternion.identity;
            
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.transform.SetParent(staff.transform);
            shaft.transform.localPosition = new Vector3(0, 0.75f, 0);
            shaft.transform.localScale = new Vector3(0.08f, 1.5f, 0.08f);
            
            Renderer shaftRenderer = shaft.GetComponent<Renderer>();
            if (shaftRenderer != null)
            {
                Material shaftMaterial = new Material(Shader.Find("Standard"));
                shaftMaterial.color = Color.brown;
                shaftRenderer.material = shaftMaterial;
            }
            
            GameObject bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.transform.SetParent(staff.transform);
            bell.transform.localPosition = new Vector3(0, 1.5f, 0);
            bell.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            
            Renderer bellRenderer = bell.GetComponent<Renderer>();
            if (bellRenderer != null)
            {
                Material bellMaterial = new Material(Shader.Find("Standard"));
                bellMaterial.color = Color.yellow;
                bellMaterial.SetFloat("_Metallic", 0.9f);
                bellRenderer.material = bellMaterial;
            }
            
            return staff;
        }

        private GameObject CreateDaggerPlaceholder(Transform parent)
        {
            GameObject dagger = new GameObject("Dagger");
            dagger.transform.SetParent(parent);
            dagger.transform.localPosition = Vector3.zero;
            dagger.transform.localRotation = Quaternion.identity;
            
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.SetParent(dagger.transform);
            blade.transform.localPosition = new Vector3(0, 0.3f, 0);
            blade.transform.localScale = new Vector3(0.05f, 0.6f, 0.02f);
            
            Renderer bladeRenderer = blade.GetComponent<Renderer>();
            if (bladeRenderer != null)
            {
                Material bladeMaterial = new Material(Shader.Find("Standard"));
                bladeMaterial.color = Color.black;
                bladeMaterial.SetFloat("_Metallic", 0.8f);
                bladeRenderer.material = bladeMaterial;
            }
            
            return dagger;
        }

        private GameObject CreateSpearPlaceholder(Transform parent)
        {
            GameObject spear = new GameObject("Spear");
            spear.transform.SetParent(parent);
            spear.transform.localPosition = Vector3.zero;
            spear.transform.localRotation = Quaternion.identity;
            
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.transform.SetParent(spear.transform);
            shaft.transform.localPosition = new Vector3(0, 1f, 0);
            shaft.transform.localScale = new Vector3(0.05f, 2f, 0.05f);
            
            GameObject spearhead = GameObject.CreatePrimitive(PrimitiveType.Cone);
            spearhead.transform.SetParent(spear.transform);
            spearhead.transform.localPosition = new Vector3(0, 2f, 0);
            spearhead.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            
            return spear;
        }

        private GameObject CreateLutePlaceholder(Transform parent)
        {
            GameObject lute = new GameObject("Lute");
            lute.transform.SetParent(parent);
            lute.transform.localPosition = Vector3.zero;
            lute.transform.localRotation = Quaternion.identity;
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(lute.transform);
            body.transform.localPosition = new Vector3(0, 0.5f, 0);
            body.transform.localScale = new Vector3(0.4f, 0.6f, 0.2f);
            
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.transform.SetParent(lute.transform);
            neck.transform.localPosition = new Vector3(0, 1f, 0);
            neck.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            
            return lute;
        }

        private GameObject CreateHammerPlaceholder(Transform parent)
        {
            GameObject hammer = new GameObject("Hammer");
            hammer.transform.SetParent(parent);
            hammer.transform.localPosition = Vector3.zero;
            hammer.transform.localRotation = Quaternion.identity;
            
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.transform.SetParent(hammer.transform);
            handle.transform.localPosition = new Vector3(0, 0.5f, 0);
            handle.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
            
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(hammer.transform);
            head.transform.localPosition = new Vector3(0, 1f, 0);
            head.transform.localScale = new Vector3(0.4f, 0.3f, 0.3f);
            
            return hammer;
        }

        private Transform FindWeaponAttachPoint()
        {
            if (currentModel == null) return transform;
            
            Transform rightHand = FindChildRecursive(currentModel.transform, "RightHand");
            if (rightHand == null)
            {
                rightHand = FindChildRecursive(currentModel.transform, "Hand_R");
            }
            if (rightHand == null)
            {
                rightHand = FindChildRecursive(currentModel.transform, "mixamorig:RightHand");
            }
            
            if (rightHand == null)
            {
                GameObject weaponAttach = new GameObject("WeaponAttach");
                weaponAttach.transform.SetParent(currentModel.transform);
                weaponAttach.transform.localPosition = new Vector3(0.5f, 1f, 0);
                rightHand = weaponAttach.transform;
            }
            
            return rightHand;
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                    
                Transform found = FindChildRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void SetupAnimatorController(string characterClass)
        {
            if (animator == null) return;
            
            RuntimeAnimatorController controller = CreateBasicAnimatorController();
            animator.runtimeAnimatorController = controller;
        }

        private RuntimeAnimatorController CreateBasicAnimatorController()
        {
            return null;
        }

        private void SetupCharacterComponents()
        {
            if (currentModel == null) return;
            
            Collider collider = currentModel.GetComponent<Collider>();
            if (collider == null)
            {
                CapsuleCollider capsule = currentModel.AddComponent<CapsuleCollider>();
                capsule.height = 2f;
                capsule.radius = 0.5f;
                capsule.center = new Vector3(0, 1f, 0);
            }
            
            Rigidbody rb = currentModel.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentModel.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
                rb.mass = 1f;
            }
        }

        public void PlayAnimation(string animationName)
        {
            if (animator != null)
            {
                animator.SetTrigger(animationName);
                Debug.Log($"🎬 Playing animation: {animationName}");
            }
        }

        public void SetAnimationBool(string parameterName, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(parameterName, value);
            }
        }

        public void SetAnimationFloat(string parameterName, float value)
        {
            if (animator != null)
            {
                animator.SetFloat(parameterName, value);
            }
        }
    }
}

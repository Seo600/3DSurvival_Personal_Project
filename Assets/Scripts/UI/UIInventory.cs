using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;
    
    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;

    [Header("Select Item")] 
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedStatName;
    public TextMeshProUGUI selectedStatValue;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unequipeButton;
    public GameObject dropButton;

    private PlayerController controller;
    private PlayerCondition condition;
    
    ItemData selectedItem;
    int selectedItemIndex = 0;

    int curEquipIndex;
    
    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;

        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;
        
        inventoryWindow.SetActive(false);
        slots = new ItemSlot[slotPanel.childCount];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
        }

        ClearSelectedItemWindow();
    }

    void ClearSelectedItemWindow()
    {
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedStatName.text = string.Empty;
        selectedStatValue.text = string.Empty;
        
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unequipeButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void Toggle()        //인벤토리 껏다켰다하는 함수
    {
        if (IsOpen())
        {
            inventoryWindow.SetActive(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData;
        //아이템 중복이 가능한지
        if (data.canStack)
        {
            ItemSlot slot = GetItemStack(data);
            if (slot != null)       //인벤토리에 해당하는 오브젝트가 있다면
            {
                slot.quantity++;        //갯수 증가
                UpdateUI();
                CharacterManager.Instance.Player.itemData = null;
                return;
            }
        }
        //slot이 null이면 새로만들어야 되니 빈슬롯을 가져온다.
        //비어있는 슬롯 가져온다
        ItemSlot emptySlot = GetEmptySlot();
        
        //있다면
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }
        //더 가져올 슬롯이 없으면 버린다.
        
        //없다면
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    void UpdateUI()         //인벤토리에 UI를 업데이트 
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)       //데이터가 들어가 있다면
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }
        
    ItemSlot GetItemStack(ItemData data)        //아이템을 슬롯에 넣는다
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }

    ItemSlot GetEmptySlot()             //비어있는 슬롯 가져오기 
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    ItemSlot ThrowItem(ItemData data)   //파밍한 아이템 버리기
    {
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
        return null;
    }

    //아이템 슬롯에서 버튼을 눌렀을때 SelectItem함수가 호출이 되면서 UI가 세팅이 된다.
    public void SelectItem(int index)
    {
        if (slots[index].item == null) return;

        selectedItem = slots[index].item;
        selectedItemIndex = index;
        
        selectedItemName.text = selectedItem.displayName;
        selectedItemDescription.text = selectedItem.description;

        selectedStatName.text = string.Empty;
        selectedStatValue.text = string.Empty;

        for (int i = 0; i < selectedItem.consumables.Length; i++)
        {
            selectedStatName.text += selectedItem.consumables[i].type.ToString() + "\n";
            selectedStatValue.text += selectedItem.consumables[i].value.ToString() + "\n";
        }
        
        useButton.SetActive(selectedItem.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.type == ItemType.Equipable && !slots[index].equipped);   //장착이 안되있을때는 장착이라는 버튼 켜주기
        unequipeButton.SetActive(selectedItem.type == ItemType.Equipable&& slots[index].equipped);  //장착이 되어있을때에는 해체라는 버튼 켜주기
        dropButton.SetActive(true);
    }

    public void OnUseButton()
    {
        if (selectedItem.type == ItemType.Consumable)
        {
            for (int i = 0; i < selectedItem.consumables.Length; i++)
            {
                switch (selectedItem.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.consumables[i].value);
                        break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.consumables[i].value);
                        break;
                    case ConsumableType.Speed:
                        controller.PlusSpeed(selectedItem.consumables[i].time, selectedItem.consumables[i].value);
                        break;
                }
            }
            RemoveSelectedItem();
        }
    }

    public void OnDropButton()
    {
        ThrowItem(selectedItem);
        RemoveSelectedItem();
    }

    void RemoveSelectedItem()
    {
        slots[selectedItemIndex].quantity--;

        if (slots[selectedItemIndex].quantity <= 0)
        {
            selectedItem = null;
            slots[selectedItemIndex].item = null;       //인벤토리 UI아이템 사라지게하는 코드
            selectedItemIndex = -1;
            ClearSelectedItemWindow();
        }
        
        UpdateUI();
    }
}

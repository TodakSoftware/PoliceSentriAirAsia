using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{
    public SO_AnimatorVariant policeSO;
    public SO_AnimatorVariant robberSO;
    public SO_PetVariant petSO;
    public List<UI_ShopTab> shopTabLists = new List<UI_ShopTab>();
    public Transform content;
    public GameObject btnShopPrefab;
    public int currentTabIndex;
    List<GameObject> storeList = new List<GameObject>();

    [Header("Preview")]
    public int currentSelectedIndex;
    public string currentSelectedCode;
    public float currentSelectedPrice;
    public TextMeshProUGUI charNameText;
    public Image charPreviewImage;
    public Button purchaseBtn;
    public TextMeshProUGUI purchaseText;

    [Header("Confirm Buy")]
    public TextMeshProUGUI kupangText;
    public ConfirmBuy confirmBuy;

    void Start(){
        confirmBuy.btnYes.onClick.AddListener( delegate{ PurchaseItem(); }); // link btnYes to function
        SetupTabIndex();
        OpenShopTab(0, 0);
        kupangText.text = UserDataManager.instance.currentKupang.ToString("N0");
    }

    void SetupTabIndex(){
        for(int i = 0; i < shopTabLists.Count; i++){
            if(shopTabLists[i] != null){
                shopTabLists[i].index = i;
                shopTabLists[i].shopRef = this;
            }
        } // end for
    } // end SetupTabIndex

    public void OpenShopTab(int _index, int _btnIndex){
        foreach(var tab in shopTabLists){
            tab.ButtonDeselected();
        }

        shopTabLists[_index].ButtonSelected();
        currentTabIndex = _index;
        ListContent(_index);

        if(storeList.Count > 0){
            storeList[_btnIndex].GetComponent<Btn_Shop>().ButtonSelected();
        }
    } // end OpenShopTab

    void ClearContent(){
        if(content.childCount > 0){
            foreach(Transform child in content){
                Destroy(child.gameObject);
            }
        }
    } // end ClearContent

    void ClearShopButton(){
        if(content.childCount > 0){
            foreach(Transform child in content){
                child.GetComponent<Btn_Shop>().ButtonUnselected();
            }
        }
    } // end ClearContent

    void ListContent(int _index){
        ClearContent(); // Clear existing child content
        storeList.Clear();

        switch(_index){
            case 0: // Police
                if(policeSO.animatorLists.Count > 0){
                    var ind = 0;
                    foreach(var item in policeSO.animatorLists){
                        var btn = Instantiate(btnShopPrefab);
                        btn.GetComponent<Btn_Shop>().shopRef = this;
                        btn.GetComponent<Btn_Shop>().btnIndex = ind;
                        btn.GetComponent<Btn_Shop>().icon.sprite = item.iconHead;
                        if(item.type == E_ValueType.PREMIUM){
                            if(!UserDataManager.instance.policeList.Contains(item.code)) // if not contain
                            btn.GetComponent<Btn_Shop>().lockIcon.SetActive(true);
                        }
                        btn.transform.SetParent(content, false);
                        ind++;
                        storeList.Add(btn);
                    }
                }
            break;

            case 1: // Robber
                if(robberSO.animatorLists.Count > 0){
                    var ind = 0;
                    foreach(var item in robberSO.animatorLists){
                        var btn = Instantiate(btnShopPrefab);
                        btn.GetComponent<Btn_Shop>().shopRef = this;
                        btn.GetComponent<Btn_Shop>().btnIndex = ind;
                        btn.GetComponent<Btn_Shop>().icon.sprite = item.iconHead;
                        if(item.type == E_ValueType.PREMIUM){
                            if(!UserDataManager.instance.robberList.Contains(item.code)) // if not contain
                            btn.GetComponent<Btn_Shop>().lockIcon.SetActive(true);
                        }
                        btn.transform.SetParent(content, false);
                        ind++;
                        storeList.Add(btn);
                    }
                }
            break;

            case 2: // Pet
                if(petSO.animatorLists.Count > 0){
                    var ind = 0;
                    foreach(var item in petSO.animatorLists){
                        var btn = Instantiate(btnShopPrefab);
                        btn.GetComponent<Btn_Shop>().shopRef = this;
                        btn.GetComponent<Btn_Shop>().btnIndex = ind;
                        btn.GetComponent<Btn_Shop>().icon.sprite = item.iconHead;
                        if(item.type == E_ValueType.PREMIUM){
                            if(!UserDataManager.instance.petList.Contains(item.code)) // if not contain
                            btn.GetComponent<Btn_Shop>().lockIcon.SetActive(true);
                        }
                        btn.transform.SetParent(content, false);
                        ind++;
                        storeList.Add(btn);
                    }
                }
            break;

            default:
            break;
        }
    } // end listContent

    public void SetPreview(int _charIndex){
        ClearShopButton();
        currentSelectedIndex = _charIndex;
        switch(currentTabIndex){
            case 0: // Police
                if(policeSO.animatorLists.Count > 0){
                    charNameText.text = policeSO.animatorLists[_charIndex].name;
                    currentSelectedCode = policeSO.animatorLists[_charIndex].code;
                    currentSelectedPrice = policeSO.animatorLists[_charIndex].kupang;
                    charPreviewImage.sprite = policeSO.animatorLists[_charIndex].idlePose;

                    if(policeSO.animatorLists[_charIndex].type == E_ValueType.FREE){
                       purchaseBtn.gameObject.SetActive(false);
                    }else{
                        if(!UserDataManager.instance.policeList.Contains(policeSO.animatorLists[_charIndex].code)){ // if not contain
                            purchaseBtn.gameObject.SetActive(true);
                            purchaseText.text = policeSO.animatorLists[_charIndex].kupang.ToString("N0");
                        }else{
                            purchaseBtn.gameObject.SetActive(false);
                        }
                    }
                }
            break;

            case 1: // Robber
                if(robberSO.animatorLists.Count > 0){
                    charNameText.text = robberSO.animatorLists[_charIndex].name;
                    currentSelectedCode = robberSO.animatorLists[_charIndex].code;
                    currentSelectedPrice = robberSO.animatorLists[_charIndex].kupang;
                    charPreviewImage.sprite = robberSO.animatorLists[_charIndex].idlePose;

                    if(robberSO.animatorLists[_charIndex].type == E_ValueType.FREE){
                       purchaseBtn.gameObject.SetActive(false);
                    }else{
                        if(!UserDataManager.instance.robberList.Contains(robberSO.animatorLists[_charIndex].code)){ // if not contain
                            purchaseBtn.gameObject.SetActive(true);
                            purchaseText.text = robberSO.animatorLists[_charIndex].kupang.ToString("N0");
                        }else{
                            purchaseBtn.gameObject.SetActive(false);
                        }
                    }
                }
            break;

            case 2: // Pet
                if(petSO.animatorLists.Count > 0){
                    charNameText.text = petSO.animatorLists[_charIndex].name;
                    currentSelectedCode = petSO.animatorLists[_charIndex].code;
                    currentSelectedPrice = petSO.animatorLists[_charIndex].kupang;
                    charPreviewImage.sprite = petSO.animatorLists[_charIndex].idlePose;

                    if(petSO.animatorLists[_charIndex].type == E_ValueType.FREE){
                       purchaseBtn.gameObject.SetActive(false);
                    }else{
                        if(!UserDataManager.instance.robberList.Contains(petSO.animatorLists[_charIndex].code)){ // if not contain
                            purchaseBtn.gameObject.SetActive(true);
                            purchaseText.text = petSO.animatorLists[_charIndex].kupang.ToString("N0");
                        }else{
                            purchaseBtn.gameObject.SetActive(false);
                        }
                    }
                }
            break;

            default:
            break;
        }
    } // setPreview

    public void OpenPurchaseWindow(){
        confirmBuy.gameObject.SetActive(true);
        var confirmScript = confirmBuy.GetComponent<ConfirmBuy>();
        switch(currentTabIndex){
            case 0: // Police
                if(policeSO.animatorLists.Count > 0){
                    print("Selected " + policeSO.animatorLists[currentSelectedIndex].name);
                    confirmScript.iconImage.sprite = policeSO.animatorLists[currentSelectedIndex].idlePose;
                    confirmScript.itemName.text = policeSO.animatorLists[currentSelectedIndex].name;
                    confirmScript.itemPrice.text = policeSO.animatorLists[currentSelectedIndex].kupang.ToString("N0");
                }
            break;

            case 1: // Robber
                if(robberSO.animatorLists.Count > 0){
                    print("Selected " + robberSO.animatorLists[currentSelectedIndex].name);
                    confirmScript.iconImage.sprite = robberSO.animatorLists[currentSelectedIndex].idlePose;
                    confirmScript.itemName.text = robberSO.animatorLists[currentSelectedIndex].name;
                    confirmScript.itemPrice.text = robberSO.animatorLists[currentSelectedIndex].kupang.ToString("N0");
                }
            break;

            case 2: // Pet
                if(petSO.animatorLists.Count > 0){
                    print("Selected " + petSO.animatorLists[currentSelectedIndex].name);
                    confirmScript.iconImage.sprite = petSO.animatorLists[currentSelectedIndex].idlePose;
                    confirmScript.itemName.text = petSO.animatorLists[currentSelectedIndex].name;
                    confirmScript.itemPrice.text = petSO.animatorLists[currentSelectedIndex].kupang.ToString("N0");
                }
            break;

        } // end switch
    } // end OpenPurchaseWindow

    public void PurchaseItem(){
        switch(currentTabIndex){
            case 0: // Police
                if(UserDataManager.instance.currentKupang >= currentSelectedPrice){
                    if(policeSO.animatorLists.Count > 0){
                        if(!UserDataManager.instance.policeList.Contains(policeSO.animatorLists[currentSelectedIndex].code)){ // if not contain, add
                            UserDataManager.instance.policeList.Add(currentSelectedCode);
                            print("Added to police");
                            // Popup notifcation
                            NotificationManager.instance.PopupNotification("Successfully Purchased!");
                            // Refresh shop button lock
                            OpenShopTab(currentTabIndex, currentSelectedIndex);
                            // Close GO
                            confirmBuy.gameObject.SetActive(false);
                            // Deduct Kupang
                            UserDataManager.instance.currentKupang -= currentSelectedPrice;
                            kupangText.text = UserDataManager.instance.currentKupang.ToString("N0");
                            // Add to firebase (Code, kupang)
                        }
                    }
                }else{
                    NotificationManager.instance.PopupNotification("Kupang Not Enough!");
                }
                
            break;

            case 1: // Robber
                if(UserDataManager.instance.currentKupang >= currentSelectedPrice){
                    if(robberSO.animatorLists.Count > 0){
                        if(!UserDataManager.instance.robberList.Contains(robberSO.animatorLists[currentSelectedIndex].code)){
                            UserDataManager.instance.robberList.Add(currentSelectedCode);
                            print("Added to robber");
                            // Popup notifcation
                            NotificationManager.instance.PopupNotification("Successfully Purchased!");
                            // Refresh shop button lock
                            OpenShopTab(currentTabIndex, currentSelectedIndex);
                            // Close GO
                            confirmBuy.gameObject.SetActive(false);
                            // Deduct Kupang
                            UserDataManager.instance.currentKupang -= currentSelectedPrice;
                            kupangText.text = UserDataManager.instance.currentKupang.ToString("N0");
                            // Add to firebase (Code, kupang)
                        }
                    }
                }else{
                    NotificationManager.instance.PopupNotification("Kupang Not Enough!");
                }
            break;

            case 2: // Pet
                if(UserDataManager.instance.currentKupang >= currentSelectedPrice){
                    if(petSO.animatorLists.Count > 0){
                        if(!UserDataManager.instance.petList.Contains(petSO.animatorLists[currentSelectedIndex].code)){
                            UserDataManager.instance.petList.Add(currentSelectedCode);
                            print("Added to pet");
                            // Popup notifcation
                            NotificationManager.instance.PopupNotification("Successfully Purchased!");
                            // Refresh shop button lock
                            OpenShopTab(currentTabIndex, currentSelectedIndex);
                            // Close GO
                            confirmBuy.gameObject.SetActive(false);
                            // Deduct Kupang
                            UserDataManager.instance.currentKupang -= currentSelectedPrice;
                            kupangText.text = UserDataManager.instance.currentKupang.ToString("N0");
                            // Add to firebase (Code, kupang)
                        }
                    }
                }else{
                    NotificationManager.instance.PopupNotification("Kupang Not Enough!");
                }
            break;

            default:
                print("Invalid Code");
            break;
        } // end switch
    } // end PurchaseItem
}

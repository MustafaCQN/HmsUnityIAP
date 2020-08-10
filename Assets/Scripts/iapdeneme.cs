using HmsPlugin;
using HuaweiMobileServices.IAP;
using HuaweiMobileServices.Id;
using HuaweiMobileServices.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;
using UnityEngine.UI;

public class iapdeneme : MonoBehaviour
{

    public AccountManager accountManager;
    public IapManager iapManager;
    public Text topInfoText;
    public Text bottomInfoText;
    public Button payButton;

    public string[] ConsumableProducts;
    public string[] NonConsumableProducts;
    public string[] SubscriptionProducts;
    public ProductInfo takenProduct;

    List<ProductInfo> productInfoList = new List<ProductInfo>();


    private void Start()
    {
        iapManager = IapManager.GetInstance("IapManager");
        accountManager = AccountManager.GetInstance("AccountManager");
        topInfoText = GameObject.Find("topInfoText").GetComponent<Text>();
        bottomInfoText = GameObject.Find("bottomInfoText").GetComponent<Text>();
        payButton = GameObject.Find("payButton").GetComponent<Button>();
        payButton.interactable = false;
        payButton.onClick.AddListener(BuyProduct);
        accountManager.SignIn();
        accountManager.OnSignInSuccess = SignInHandler;
        accountManager.OnSignInFailed = SignInFailedHandler;
    }

    void SignInHandler(AuthHuaweiId id)
    {
        //bottomInfoText.text = "";
        topInfoText.text = $"Sign-in Success, Huawei DispName: {id.DisplayName}\n";
        topInfoText.text = $"{topInfoText.text}Executing iapManager.CheckIapAvailability()...\n";
        iapManager.CheckIapAvailability();
        iapManager.OnCheckIapAvailabilitySuccess = IapAvailabilitySuccessHandler;
        iapManager.OnCheckIapAvailabilityFailure = IapAvailabilityFailureHandler;
    }

    void SignInFailedHandler(HMSException ex)
    {
        topInfoText.text = $"Sign-in Failed, ExceptionCode: {ex.ErrorCode}";
    }

    void IapAvailabilitySuccessHandler()
    {
        topInfoText.text = $"{topInfoText.text}IapAvailabilitySuccessHandler Sucess!\n";
        topInfoText.text = $"{topInfoText.text}Executing iapManager.ObtainProductInfo()...\n";
        //ProductInfoReq productInfoReq 
        // iapManager.ObtainProductInfo(new List<string>(ConsumableProducts), new List<string>(NonConsumableProducts), new List<string>(SubscriptionProducts));
        iapManager.ObtainProductInfo(ConsumableProducts, NonConsumableProducts, SubscriptionProducts);
        iapManager.OnObtainProductInfoSuccess = (productInfoResultList) =>
        {
            topInfoText.text = $"{topInfoText.text}OnObtainProductInfoSuccess Success!\n";

            foreach (ProductInfoResult result in productInfoResultList)
            {
                foreach (ProductInfo info in result.ProductInfoList)
                {
                    productInfoList.Add(info);
                }
            }

            int i = 0;
            foreach (ProductInfo info in productInfoList)
            {
                bottomInfoText.text = $"{bottomInfoText.text}Prod{i}: {info.ProductName} Prod{i}Id: {info.ProductId} Prod{i}Desc: {info.ProductDesc}\n";
                if (info.PriceType == 1)
                {
                    takenProduct = info;
                    payButton.interactable = true;
                    //BuyProduct(info);
                    bottomInfoText.text = $"{bottomInfoText.text}Product Found!, ProdName:{info.ProductName}, PriceType:{info.PriceType}\n";
                }
                i++;
            }
        };

        iapManager.OnObtainProductInfoFailure = (ex) =>
        {
            topInfoText.text = $"{topInfoText.text}OnObtainProductInfoFailure ExCode: {ex.ErrorCode}\n";
        };
    }

    void IapAvailabilityFailureHandler(HMSException ex)
    {
        topInfoText.text = $"{topInfoText.text}IapAvailabilityFailureHandler ExCode: {ex.ErrorCode}\n";
    }

    void BuyProduct()
    {
        iapManager.OnBuyProductSuccess = (purchaseResultInfo) =>
        {
            // Verify signature with purchaseResultInfo.InAppDataSignature

            // If signature ok, deliver product

            // Consume product purchaseResultInfo.InAppDataSignature
            iapManager.ConsumePurchase(purchaseResultInfo);
            bottomInfoText.text = $"{bottomInfoText.text}\nOnBuyProductSuccess\n";
        };

        iapManager.OnBuyProductFailure = (errorCode) =>
        {
            switch (errorCode)
            {
                case OrderStatusCode.ORDER_STATE_CANCEL:
                    // User cancel payment.
                    Debug.Log("[HMS]: User cancel payment");
                    bottomInfoText.text = $"{bottomInfoText.text}User Cancelled the Payment\n";
                    break;
                case OrderStatusCode.ORDER_STATE_FAILED:
                    Debug.Log("[HMS]: order payment failed");
                    bottomInfoText.text = $"{bottomInfoText.text}Order Payment Failed\n";
                    break;
                case OrderStatusCode.ORDER_PRODUCT_OWNED:
                    Debug.Log("[HMS]: Product owned");
                    bottomInfoText.text = $"{bottomInfoText.text}Product Owned\n";
                    break;
                default:
                    Debug.Log("[HMS:] BuyProduct ERROR" + errorCode);
                    bottomInfoText.text = $"{bottomInfoText.text}Buy Product Error Code:{errorCode}\n";
                    break;
            }
        };

        ProductInfo info = takenProduct;
        string payload = "test";
        bottomInfoText.text = $"{bottomInfoText.text}Executing iapManager.BuyProduct()\n";
        iapManager.BuyProduct(info, payload);
    }
}

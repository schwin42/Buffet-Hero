using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Soomla.Store {



	/// <summary>
	/// This class defines our game's economy, which includes virtual goods, virtual currencies
	/// and currency packs, virtual categories
	/// </summary>
	public class StoreController : IStoreAssets{

		/** LifeTimeVGs **/
		// Note: create non-consumable items using LifeTimeVG with PuchaseType of PurchaseWithMarket
		public static VirtualGood NO_ADS_LTVG = new LifetimeVG(
			"No Ads", 														// name
			"No More Ads!",				 									// description
			"no_ads",														// item id
			new PurchaseWithMarket("01RemoveAds", 1.99));	// the way this virtual good is purchased

		public  int GetVersion ()
		{
			return 0;
		}
		
		public  VirtualCurrency[] GetCurrencies()
		{
			return new VirtualCurrency[0];
		}

		/// <summary>
		/// Retrieves the array of all virtual goods served by your store (all kinds in one array).
		/// </summary>
		/// <returns>All virtual goods in your game.</returns>
		public  VirtualGood[] GetGoods()
		{
			return new VirtualGood[1]
			{
				NO_ADS_LTVG
			};
		}
		
		/// <summary>
		/// Retrieves the array of all virtual currency packs served by your store.
		/// </summary>
		/// <returns>All virtual currency packs in your game.</returns>
		public  VirtualCurrencyPack[] GetCurrencyPacks() 
		{
			return new VirtualCurrencyPack[0];
		}
		
		/// <summary>
		/// Retrieves the array of all virtual categories handled in your store.
		/// </summary>
		/// <returns>All virtual categories in your game.</returns>
		public VirtualCategory[] GetCategories()
		{
			return new VirtualCategory[0];
		}


	}
	
}
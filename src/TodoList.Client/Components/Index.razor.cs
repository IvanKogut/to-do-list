﻿using API.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList.Client.Components
{
  public class IndexComponent : ComponentBase
  {
    [Inject]
    private IAppHttpClient AppHttpClient { get; set; }

    [Inject]
    private ILocalStorageService LocalStorageService { get; set; }

    [Inject]
    private IUriHelper UriHelper { get; set; }

    protected ItemCreateApiModel NewItem { get; set; }
    protected IList<ItemApiModel> Items { get; set; }

    protected override async Task OnInitAsync()
    {
      NewItem = new ItemCreateApiModel();
      Items = (await AppHttpClient.GetAsync<IList<ItemApiModel>>(ApiUrls.GetItemsList)).Value;
    }

    protected async Task OnCreateItemAsync()
    {
      if (!string.IsNullOrWhiteSpace(NewItem.Text))
      {
        ApiCallResult<ItemApiModel> itemCreationCallResult = await AppHttpClient
          .PostAsync<ItemApiModel>(ApiUrls.CreateItem, NewItem);

        Items.Add(itemCreationCallResult.Value);
        NewItem.Text = string.Empty;
      }
    }

    protected async Task UpdateItemStatusAsync(UIChangeEventArgs e, ItemApiModel item)
    {
      item.IsDone = (bool)e.Value;

      await UpdateItemAsync(item);
    }

    protected async Task UpdateItemTextAsync(UIChangeEventArgs e, ItemApiModel item)
    {
      item.Text = (string)e.Value;

      await UpdateItemAsync(item);
    }

    protected async Task DeleteItemAsync(ItemApiModel item)
    {
      Items.Remove(item);

      await AppHttpClient.DeleteAsync(ApiUrls.DeleteItem.Replace(Urls.DeleteItem, item.Id.ToString()));
    }

    protected async Task MoveUpItemAsync(ItemApiModel item)
    {
      int indexOfItem = Items.IndexOf(item);
      int indexOfPrevItem = indexOfItem - 1;

      await SwapItemsAsync(item, indexOfItem, indexOfPrevItem);
    }

    protected async Task MoveDownItemAsync(ItemApiModel item)
    {
      int indexOfItem = Items.IndexOf(item);
      int indexOfNextItem = indexOfItem + 1;

      await SwapItemsAsync(item, indexOfItem, indexOfNextItem);
    }

    protected async Task OnLogoutAsync()
    {
      await LocalStorageService.RemoveItemAsync(AppState.AuthTokenKey);

      UriHelper.NavigateTo(string.Empty);
    }

    private Task SwapItemsAsync(ItemApiModel item, int indexOfSelectedItem, int indexOfAnotherItem)
    {
      ItemApiModel anotherItem = Items[indexOfAnotherItem];

      Items[indexOfSelectedItem] = anotherItem;
      Items[indexOfAnotherItem] = item;

      int itemPriority = item.Priority;

      item.Priority = anotherItem.Priority;
      anotherItem.Priority = itemPriority;

      return Task.WhenAll(UpdateItemAsync(item), UpdateItemAsync(anotherItem));
    }

    private Task UpdateItemAsync(ItemApiModel item)
    {
      return AppHttpClient.PutAsync(ApiUrls.UpdateItem.Replace(Urls.UpdateItem, item.Id.ToString()), item);
    }
  }
}
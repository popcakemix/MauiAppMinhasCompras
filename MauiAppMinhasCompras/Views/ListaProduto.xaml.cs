using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;

namespace MauiAppMinhasCompras.Views
{
    public partial class ListaProduto : ContentPage
    {
        ObservableCollection<Produto> lista = new ObservableCollection<Produto>();

        public ListaProduto()
        {
            InitializeComponent();
            lst_produtos.ItemsSource = lista;
        }

        protected async override void OnAppearing()
        {
            await CarregarLista();
        }

        private async Task CarregarLista()
        {
            lista.Clear();
            List<Produto> tmp = await App.Db.GetAll();
            tmp.ForEach(i => lista.Add(i));

            var categorias = tmp.Select(p => p.Categoria)
                                .Where(c => !string.IsNullOrEmpty(c))
                                .Distinct().ToList();
            pck_categoria.ItemsSource = categorias;
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            try { Navigation.PushAsync(new Views.NovoProduto()); }
            catch (Exception ex) { DisplayAlert("Ops", ex.Message, "OK"); }
        }

        private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string q = e.NewTextValue;
                lista.Clear();
                List<Produto> tmp = await App.Db.Search(q);
                tmp.ForEach(i => lista.Add(i));
            }
            catch (Exception ex) { DisplayAlert("Ops", ex.Message, "OK"); }
        }

        private async void pck_categoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selecionada = pck_categoria.SelectedItem as string;
                if (!string.IsNullOrEmpty(selecionada))
                {
                    List<Produto> todos = await App.Db.GetAll();
                    var filtrados = todos.Where(p => p.Categoria == selecionada).ToList();
                    lista.Clear();
                    filtrados.ForEach(i => lista.Add(i));
                }
            }
            catch (Exception ex) { await DisplayAlert("Ops", ex.Message, "OK"); }
        }

        private async void BtnRelatorio_Clicked(object sender, EventArgs e)
        {
            try
            {
                var produtos = await App.Db.GetAll();
                var resumo = produtos.GroupBy(p => p.Categoria ?? "Sem Categoria")
                                     .Select(g => $"{g.Key}: {g.Sum(p => p.Total):C}")
                                     .ToList();
                await DisplayAlert("Gastos por Categoria", string.Join("\n", resumo), "OK");
            }
            catch (Exception ex) { await DisplayAlert("Ops", ex.Message, "OK"); }
        }

        private void ToolbarItem_Clicked_1(object sender, EventArgs e)
        {
            double soma = lista.Sum(i => i.Total);
            DisplayAlert("Total", $"Valor total: {soma:C}", "OK");
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            MenuItem selecionado = sender as MenuItem;
            Produto p = selecionado.BindingContext as Produto;
            if (await DisplayAlert("Confirmação", $"Excluir {p.Descricao}?", "Sim", "Não"))
            {
                await App.Db.Delete(p.Id);
                lista.Remove(p);
            }
        }

        private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Produto p = e.SelectedItem as Produto;
            Navigation.PushAsync(new Views.EditarProduto { BindingContext = p });
        }

        private async void lst_produtos_Refreshing(object sender, EventArgs e)
        {
            await CarregarLista();
            lst_produtos.IsRefreshing = false;
        }
    }
}
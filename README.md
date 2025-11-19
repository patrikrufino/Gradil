# Gradil (WPF)

Aplicativo WPF para gerar "gradils" (posterização de PDFs em tiles).

Status: esqueleto criado. A lógica de renderização de PDF ainda precisa ser implementada.

Dependências NuGet sugeridas (já instaladas por você):

- `FluentWPF`
- `CommunityToolkit.Mvvm` (opcional)
- `PdfiumSharp` (ou alternativa)
- `PdfSharpCore`
- `Microsoft.Windows.SDK.Contracts`

Comandos para reinstalar/confirmar pacotes (no diretório do `.csproj`):

```powershell
dotnet add package FluentWPF
dotnet add package CommunityToolkit.Mvvm
dotnet add package PdfiumSharp
dotnet add package PdfSharpCore
dotnet add package Microsoft.Windows.SDK.Contracts
```

Arquivos adicionados (esqueleto):

- `Models/TiledPosterOptions.cs`
- `Services/IPdfService.cs`
- `Services/PdfiumPdfService.cs` (placeholder)
- `Services/PosterGenerator.cs` (orquestrador, cria placeholder .pdf)
- `ViewModels/RelayCommand.cs`
- `ViewModels/MainViewModel.cs`
- `Views/MainWindow.xaml` + `MainWindow.xaml.cs`
- `Resources/Spacing.xaml` (Fibonacci spacing)
- `Resources/Theme.xaml` (accent brush placeholder)

Próximos passos recomendados:

1. Implementar `PdfiumPdfService` usando `PdfiumSharp` para abrir PDFs e renderizar áreas de página para `Bitmap`.
2. Atualizar `PosterGenerator.GenerateAsync` para usar `IPdfService` e montar um PDF real (por exemplo com `PdfSharpCore`) a partir das imagens renderizadas.
3. Ajustar DPI/qualidade, gerenciamento de memória (descarte de bitmaps), e testar com PDFs reais.
4. Aplicar tema Fluent e ler a cor de acento do Windows no startup (usar `Windows.UI.ViewManagement.UISettings` via `Microsoft.Windows.SDK.Contracts`).

Como testar rapidamente:

1. Abra o projeto no Visual Studio ou rode `dotnet build`.
2. Execute a aplicação (F5 no Visual Studio ou `dotnet run` se for um projeto executável WPF configurado).
3. Use o botão "Selecionar arquivo" para escolher um PDF e clique em "Gerar Gradil". O gerador atual criará um arquivo placeholder em `Documents\\Gradil`.

Observações técnicas:

- O `Pdfium` requer binários nativos (x86/x64). Siga a documentação do `PdfiumSharp` para incluir as DLLs corretas no output.
- Mantenha a geração tile-a-tile para reduzir uso de memória em PDFs grandes.

Se quiser, posso já implementar a integração com `PdfiumSharp` e gerar o PDF real — deseja que eu prossiga com isso agora?

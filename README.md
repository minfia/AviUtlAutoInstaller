# AviUtl Auto Installer
AviUtlの動画作成環境を自動で行うツールです。  
このツールでは以下のことができます。
- AviUtlの環境構築
- 環境の再構築
- インストールするプラグイン/スクリプトの取捨選択
- ユーザー独自のインストール項目の作成

## 動作環境
* Windows7
* Windows8.1
* Windows10

このツールは、.Net Framework4.8のインストールが必要です(Windows10は標準でインストール)。

## 注意
このツールは<u>**無保証**</u>で提供されます。  
このツールを使用の有無に関わらず発生した全ての損害に関して保証しません。

## ダウンロード
[注意](#注意)を了承された方は、以下のURLからダウンロードしてください。  
[https://github.com/minfia/AviUtlAutoInstaller/releases](https://github.com/minfia/AviUtlAutoInstaller/releases)  
AviUtlAutoInstallerで使用しているaai.repoは[こちら](https://github.com/minfia/AAI_Repo/releases)から  

## 使い方
付属の`manual/index.html`または[こちら](https://minfia.github.io/AviUtlAutoInstaller/)を参照してください。  
軽くですが使い方を[ここ](https://www.nicovideo.jp/watch/sm39152679)で説明しています。  

## 開発環境
|           ツール名           | version |     備考     |
| ---------------------------- | ------- | ------------ |
| Windows10 Pro                | 20H2    | 動作確認含む |
| Visual Studio Community 2019 | 16.11   | 言語はC#7.3  |
| .Net Framework               | 4.8     |              |

## 使用ライブラリ
以下のライブラリを使用しています。(名前順)
* [7-zip](https://sevenzip.osdn.jp/)  
ライセンス: [Licenses/7-zip.txt](Licenses/7-zip.txt)
* [AngleSharp](https://github.com/AngleSharp/AngleSharp)  
ライセンス: [Licenses/AngleSharp.txt](Licenses/AngleSharp.txt)
* [ControlzEx](https://github.com/ControlzEx/ControlzEx)  
ライセンス: [Licenses/ControlzEx.txt](Licenses/ControlzEx.txt)
* [Costura.Fody](https://github.com/Fody/Costura)  
ライセンス: [Licenses/Costura.Fody.txt](Licenses/Costura.Fody.txt)
* [Fody](https://github.com/Fody/Home/)  
ライセンス: [Licenses/Fody.txt](Licenses/Fody.txt)
* [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)  
ライセンス: [Licenses/MahApps.Metro.txt](Licenses/MahApps.Metro.txt)
* [Nett](https://github.com/paiden/Nett)  
ライセンス: [Licenses/Nett.txt](Licenses/Nett.txt)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)  
ライセンス: [Licenses/Newtonsoft.Json.txt](Licenses/Newtonsoft.Json.txt)
* [SevenZipExtractor](https://github.com/adoconnection/SevenZipExtractor)  
ライセンス: [Licenses/SevenZipExtractor.txt](Licenses/SevenZipExtractor.txt)
* [System.Runtime.CompilerServices.Unsafe](https://github.com/dotnet/runtime)  
ライセンス: [Licenses/System.Runtime.CompilerServices.Unsafe.txt](Licenses/System.Runtime.CompilerServices.Unsafe.txt)
* [System.Text.Encoding.CodePages](https://github.com/dotnet/runtime)  
ライセンス: [Licenses/System.Text.Encoding.CodePages.txt](Licenses/System.Text.Encoding.CodePages.txt)
* [XamlBehaviorsWpf](https://github.com/microsoft/XamlBehaviorsWpf)  
ライセンス: [Licenses/XamlBehaviorsWpf.txt](Licenses/XamlBehaviorsWpf.txt)


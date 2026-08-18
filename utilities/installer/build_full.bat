del setup_full.wixobj
del setup_full.msi
c:\sift\utilities\installer\wix\candle.exe -ext "Microsoft.Tools.WindowsInstallerXml.Extensions.NetFxCompiler, WixNetFxExtension" setup_full.wxs
c:\sift\utilities\installer\wix\light.exe -ext "Microsoft.Tools.WindowsInstallerXml.Extensions.NetFxCompiler, WixNetFxExtension" C:\sift\utilities\installer\wix\netfx.wixlib -out setup_full.msi setup_full.wixobj C:\sift\utilities\installer\wix\wixui.wixlib -loc C:\sift\utilities\installer\wix\WixUI_en-us.wxl C:\sift\utilities\installer\wix\DIFxApp.wixlib c:\sift\utilities\installer\wix\wixca.wixlib
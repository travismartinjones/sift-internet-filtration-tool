del setup_basic.wixobj
del setup_basic.msi
c:\sift\utilities\installer\wix\candle.exe -ext "Microsoft.Tools.WindowsInstallerXml.Extensions.NetFxCompiler, WixNetFxExtension" setup_basic.wxs
c:\sift\utilities\installer\wix\light.exe -ext "Microsoft.Tools.WindowsInstallerXml.Extensions.NetFxCompiler, WixNetFxExtension" C:\sift\utilities\installer\wix\netfx.wixlib -out setup_basic.msi setup_basic.wixobj C:\sift\utilities\installer\wix\wixui.wixlib -loc C:\sift\utilities\installer\wix\WixUI_en-us.wxl C:\sift\utilities\installer\wix\DIFxApp.wixlib c:\sift\utilities\installer\wix\wixca.wixlib
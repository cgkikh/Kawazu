dotnet publish -r win-x64 --self-contained true -c Release -o out --framework net8.0 /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true
zip -r out.zip out/
curl -F "file=@out.zip" https://temp.sh/upload
# NrsSpear

ペネトレーションテスト補助ツールです。  
リクエストの一部パラメータを前もって設定した値に書き換えたリクエストを複数送ることができます。

# Setting

## PierceFile

リクエストの設定ファイルです。

```
{
	"Duration" : 20,
	"OutputPath" : "",

	"Url" : "http://localhost:2972/api/values",
	"Method" : "post",
	"Spears" : ["sqlInjection"],
	"Targets" : ["Value1", "Value2"],

	"Content" : 
	{
		"Value1" : "This is default value",
		"Value2" : "This is default value2"
	},
	"Headers" : 
	{ 
		"key": "This is header value" 
	},
	"Cookie" : 
	{
		"session" : "sessionid1234567890qwertyuiop"
	}
}
```

- Duration
リクエスト送信間隔です。

- OutputPath
リクエストとレスポンスの結果を納めるフォルダを指定します。

- Url
リクエストの URL を設定します。

- Method
get / post / put / delete を設定します。

- Spears
後述のSpearFileのファイル名を設定します。

- Targets
Content で書き換えを行う値を指定します。

- Content
get の場合はクエリ文字列、それ以外はボディに設定される値です。

- Headers
リクエストヘッダに設定したい値を指定します。

- Cookie
リクエスト時のクッキーを指定します。

## SpearFile

実際に書き換える値のグループファイルです。

```
Mode: Append
-----
' and 'a' = 'a
' and 'a' = 'b
 and 1 = 1
 and 1 = 0

```

- Mode
Append / Replace　を指定します。  
Append は元の値のサフィックスとして値を追加します。  
Replace は元の値を完全に置き換えます。  
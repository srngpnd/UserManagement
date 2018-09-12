Please note the following points

- Only EntityFramework used
- Have thrown exception for Insufficient Permission wherever required 
- The rest API calls work only when there is an Authorization header.
- Have seeded a default super user (Bruce) with Authorization Header as YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE=. It has all access.
- The Authorization Header is Base64 encrypted of [Email:Password] of the User
- Audits have been created
- Only few validations are added (None to the request object)
- Get,Delete,Post are based on User Permission. Any user can Patch or List.
- Sample Requests

-------- List Users -----------

curl --request GET \
  --url http://localhost:17332/api/user \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE=' \
  --header 'content-type: application/json'

----------- Get User --------------

curl --request GET \
  --url 'http://localhost:17332/api/user/Get?email=batman%40batman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE='


-------- Save Users ------------

curl --request PUT \
  --url http://localhost:17332/api/user \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE=' \
  --header 'content-type: application/json' \
  --data '[
	{
		"FirstName": "Tony",
		"LastName": "Stark",
		"Email": "ironman@ironman.com",
		"Password": "Stark",
		"Active": true,
		"Locked": false,
		"AllowedUserAccess": ["ironman@ironman.com"]
	},
	{
		"FirstName": "Clark",
		"LastName": "Kent",
		"Email": "superman@superman.com",
		"Password": "Kent",
		"Active": true,
		"Locked": false,
		"AllowedUserAccess": ["superman@superman.com","ironman@ironman.com"]
	},
	{
		"FirstName": "Steve",
		"LastName": "Rogers",
		"Email": "captainamerica@captainamerica.com",
		"Password": "Rogers",
		"Active": true,
		"Locked": false,
		"AllowedUserAccess": ["superman@superman.com","ironman@ironman.com","captainamerica@captainamerica.com"]
	},
	{
		"FirstName": "Bucky",
		"LastName": "Barnes",
		"Email": "supersoldier@supersoldier.com",
		"Password": "Stark",
		"Active": true,
		"Locked": false,
		"AllowedUserAccess": ["supersoldier@supersoldier.com","captainamerica@captainamerica.com"]
	}
]'

---------- Update User -----------

curl --request POST \
  --url 'http://localhost:17332/api/user?email=batman%40batman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE=' \
  --header 'content-type: application/json' \
  --data '{
		"FirstName": "Bruce1",
		"LastName": "Wayne1",
		"Active": false,
		"Locked": true
}'

----------- Patch User Security ----------

curl --request PATCH \
  --url 'http://localhost:17332/api/user?email=batman%40batman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE=' \
  --header 'content-type: application/json' \
  --data '[
	"batman@batman.com"
]'

----------- Get User Security ----------

curl --request GET \
  --url 'http://localhost:17332/api/user/usersecurities?email=batman%40batman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE='

----------- Delete User ------------

curl --request DELETE \
  --url 'http://localhost:17332/api/user?email=superman%40superman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE='

----------- View User Audit Logs -----------

curl --request GET \
  --url 'http://localhost:17332/api/user/getuserauditlogs?email=batman%40batman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE='


----------- View User Security Audit Logs -----------

curl --request GET \
  --url 'http://localhost:17332/api/user/getusersecurityauditlogs?email=batman%40batman.com' \
  --header 'authorization: YmF0bWFuQGJhdG1hbi5jb206V2VsY29tZTE='

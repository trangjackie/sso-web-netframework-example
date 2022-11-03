export access_token=$1
# echo "First arg: $1"
curl -v -X GET \
   http://localhost:52716/api/values \
   -H "Authorization: Bearer "$access_token
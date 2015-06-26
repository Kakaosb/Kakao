json.array!(@tags) do |tag|
  json.extract! tag, :id, :title
  json.url tag_url(tag, format: :json)
end

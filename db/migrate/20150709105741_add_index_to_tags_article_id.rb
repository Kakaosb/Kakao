class AddIndexToTagsArticleId < ActiveRecord::Migration
  def change
  	add_index :tags, :article_id
  end
end

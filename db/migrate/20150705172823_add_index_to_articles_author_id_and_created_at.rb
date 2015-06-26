class AddIndexToArticlesAuthorIdAndCreatedAt < ActiveRecord::Migration
  def change
  	add_index :articles, [:author_id, :created_at]
  end
end

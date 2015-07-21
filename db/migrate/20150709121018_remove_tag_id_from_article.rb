class RemoveTagIdFromArticle < ActiveRecord::Migration
  def change
  	remove_column :articles, :tag_id, :integer
  end
end

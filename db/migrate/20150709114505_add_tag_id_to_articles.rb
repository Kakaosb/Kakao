class AddTagIdToArticles < ActiveRecord::Migration
  def change
    add_column :articles, :tag_id, :integer

    add_index  :articles, :tag_id
  end
end

class ChangeTableArticlesTags < ActiveRecord::Migration
  def change
  	rename_table :table_articles_tags, :articles_tags
  end
end

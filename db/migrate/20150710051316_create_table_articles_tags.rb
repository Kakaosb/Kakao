class CreateTableArticlesTags < ActiveRecord::Migration
  def change
    create_table :table_articles_tags, id: false do |t|
    	t.belongs_to :article, index: true
    	t.belongs_to :tag, index: true
    end
  end
end

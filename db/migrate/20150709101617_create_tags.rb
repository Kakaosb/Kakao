class CreateTags < ActiveRecord::Migration
  def change
    create_table :tags do |t|
      t.string :tags
      t.integer :article_id
    end
  end
end

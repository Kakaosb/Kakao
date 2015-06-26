class ChangeTableArticlesAuthorToUser < ActiveRecord::Migration
  def change
  	change_table :articles do |t|

  t.rename :author_id, :user_id
end
  end
end

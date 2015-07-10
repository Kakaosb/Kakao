class Article < ActiveRecord::Base
	has_many :tags
	belongs_to :user
	default_scope -> { order('created_at DESC') }

	validates :title, length: {in: 1..20}, presence: true
	validates :text, length: {minimum: 1}, presence: true
	validates :user_id, presence: true

end

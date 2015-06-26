class Article < ActiveRecord::Base
	belongs_to :user
	
	validates :title, length: {in: 1..20}, presence: true
	validates :text, length: {minimum: 1}, presence: true

end
